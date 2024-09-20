using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityApi.Persistence.Configurations;

/// <summary>
/// Represents the <see cref="Permission"/> configuration.
/// </summary>
internal sealed class PermissionConfiguration
    : IEntityTypeConfiguration<Permission>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        
        builder.HasKey(p => p.Id);

        IEnumerable<Permission> permissions = Enum
            .GetValues<global::Domain.Enumerations.Permission>()
            .Select(p => new Permission(p.ToString())
            {
                Id = (int)p
            });

        builder.HasData(permissions);
    }
}