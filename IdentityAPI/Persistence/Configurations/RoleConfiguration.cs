using Domain.Entities;
using Identity.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityApi.Persistence.Configurations;

/// <summary>
/// Represents the <see cref="Role"/> configuration.
/// </summary>
internal sealed class RoleConfiguration
    : IEntityTypeConfiguration<Role>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        
        builder.HasKey(r => r.Value);

        builder
            .HasMany(r => r.Permissions)
            .WithMany()
            .UsingEntity<RolePermission>();

        builder.HasMany<User>()
            .WithMany();

        builder.HasData(Role.List);

    }
}