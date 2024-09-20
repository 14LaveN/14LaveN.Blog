using System.Reflection;
using Application.Core.Abstractions;
using Domain.Common.Core.Abstractions;
using Domain.Common.Core.Primitives;
using Domain.Common.Core.Primitives.Maybe;
using Domain.Core.Events;
using Domain.Core.Extensions;
using Domain.Core.Primitives;
using Identity.API.Domain.Entities;
using Identity.Domain.Enumerations;
using IdentityApi.Persistence.Configurations;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Npgsql;
using Persistence.Infrastructure;

namespace Identity.API.Persistence;

/// <summary>
/// Represents the <see cref="User"/> database context base class.
/// </summary>
public sealed class UserDbContext
    : IdentityDbContext<User, IdentityRole<Ulid>, Ulid>, IDbContext
{
    private readonly IPublisher _publisher;
    private readonly ConnectionString _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    /// <param name="publisher">The publisher.</param>
    /// <param name="connectionStringOptions">The connection string options.</param>
    public UserDbContext(
        DbContextOptions<UserDbContext> options,
        IPublisher publisher,
        ConnectionString connectionStringOptions)
        : base(options)
    {
        _connectionString = connectionStringOptions;
        _publisher = publisher;
    }

    /// <inheritdoc />
    public UserDbContext() { }
    
    /// <inheritdoc />
    public DatabaseFacade EfDatabase => Database;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables));
        optionsBuilder
            .UseNpgsql(_connectionString);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PermissionConfiguration())
            .ApplyConfiguration(new RoleConfiguration())
            .ApplyConfiguration(new UserConfiguration())
            .ApplyConfiguration(new RolePermissionConfiguration());

        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<IdentityUserLogin<Ulid>>()
           .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        modelBuilder.Entity<IdentityUserRole<Ulid>>()
             .HasKey(l => new { l.UserId, l.RoleId });
        
        modelBuilder.Entity<IdentityRole<Ulid>>(entity =>
        {
            entity.Property(e => e.Id).HasConversion(
                v => v.ToString(),
                v => Ulid.Parse(v));
        });

        modelBuilder.Entity<IdentityUserToken<Ulid>>()
            .HasKey(l => new { l.UserId, l.LoginProvider, l.Name });

        modelBuilder.Entity<Category>()
            .HasNoKey();
    }

    /// <inheritdoc />
    public new DbSet<TEntity> Set<TEntity>()
        where TEntity : class
        => base.Set<TEntity>();

    /// <exception cref="ArgumentNullException"></exception>
    /// <inheritdoc />
    public async Task<Maybe<TEntity>> GetByIdAsync<TEntity>(Ulid id)
        where TEntity : Entity
        => id == Ulid.Empty ?
            Maybe<TEntity>.None :
            Maybe<TEntity>.From(await Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id.ToString()) 
            ?? throw new ArgumentNullException());

    /// <inheritdoc />
    public async System.Threading.Tasks.Task Insert<TEntity>(TEntity entity)
        where TEntity : Entity
        => await Set<TEntity>().AddAsync(entity);

    /// <inheritdoc />
    public async System.Threading.Tasks.Task InsertRange<TEntity>(IReadOnlyCollection<TEntity> entities)
        where TEntity : Entity
        => await Set<TEntity>().AddRangeAsync(entities);

    /// <inheritdoc />
    public new async Task Remove<TEntity>(TEntity? entity)
        where TEntity : Entity
        => await Set<TEntity>()
            .WhereIf(
                entity is not null, 
                e => e.Id == entity!.Id)
            .ExecuteDeleteAsync();
    
    /// <inheritdoc />
    public Task<int> ExecuteSqlAsync(string sql, IEnumerable<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
        => Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    
   /// <summary>
   /// Saves all of the pending changes in the unit of work.
   /// </summary>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>The number of entities that have been saved.</returns>
   public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
   {
       DateTime utcNow = DateTime.UtcNow;

       UpdateAuditableEntities(utcNow);
       UpdateSoftDeletableEntities(utcNow);

       await PublishDomainEventsForIdentity(cancellationToken);

       return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the entities implementing <see cref="IAuditableEntity"/> interface.
    /// </summary>
    /// <param name="utcNow">The current date and time in UTC format.</param>
    private void UpdateAuditableEntities(DateTime utcNow)
    {
        foreach (EntityEntry<IAuditableEntity> entityEntry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(nameof(IAuditableEntity.CreatedOnUtc)).CurrentValue = utcNow;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(nameof(IAuditableEntity.ModifiedOnUtc)).CurrentValue = utcNow;
            }
        }
    }

        /// <summary>
        /// Updates the entities implementing <see cref="ISoftDeletableEntity"/> interface.
        /// </summary>
        /// <param name="utcNow">The current date and time in UTC format.</param>
        private void UpdateSoftDeletableEntities(DateTime utcNow)
        {
            foreach (EntityEntry<ISoftDeletableEntity> entityEntry in ChangeTracker.Entries<ISoftDeletableEntity>())
            {
                if (entityEntry.State != EntityState.Deleted)
                {
                    continue;
                }

                entityEntry.Property(nameof(ISoftDeletableEntity.DeletedOnUtc)).CurrentValue = utcNow;

                entityEntry.Property(nameof(ISoftDeletableEntity.Deleted)).CurrentValue = true;

                entityEntry.State = EntityState.Modified;

                UpdateDeletedEntityEntryReferencesToUnchanged(entityEntry);
            }
        }

        /// <summary>
        /// Updates the specified entity entry's referenced entries in the deleted state to the modified state.
        /// This method is recursive.
        /// </summary>
        /// <param name="entityEntry">The entity entry.</param>
        private static void UpdateDeletedEntityEntryReferencesToUnchanged(EntityEntry entityEntry)
        {
            if (!entityEntry.References.Any())
            {
                return;
            }

            foreach (ReferenceEntry referenceEntry in entityEntry.References
                         .Where(r => r.TargetEntry!.State == EntityState.Deleted))
            {
                if (referenceEntry.TargetEntry != null)
                {
                    referenceEntry.TargetEntry.State = EntityState.Unchanged;

                    UpdateDeletedEntityEntryReferencesToUnchanged(referenceEntry.TargetEntry);
                }
            }
        }
        
        /// <summary>
        /// Publishes and then clears all the domain events that exist within the current transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task PublishDomainEventsForIdentity(CancellationToken cancellationToken)
        {
            List<EntityEntry<User>> aggregateRoots = ChangeTracker
                .Entries<User>()
                .Where(entityEntry => entityEntry.Entity.DomainEvents.Count is not 0)
                .ToList();
        
            List<IDomainEvent> domainEvents = aggregateRoots
                .SelectMany(entityEntry => entityEntry.Entity.DomainEvents).ToList();

            aggregateRoots.ForEach(entityEntry => entityEntry.Entity.ClearDomainEvents());

            IEnumerable<Task> tasks = domainEvents.Select(async domainEvent => 
                await _publisher .Publish(domainEvent, cancellationToken));

            await Task.WhenAll(tasks);
        }

        /// <inheritdoc cref="FormattableString" />
        public async Task<int> ExecuteSqlAsync(FormattableString sql) =>
            await Database.ExecuteSqlAsync(sql);
}