// <copyright file="IamIdentityDbContext.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using EnterpriseIam.Identity.Domain.Tenants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIam.Identity.Infrastructure.Persistence;

/// <summary>Persists global Identity accounts and explicitly scoped tenant relationships.</summary>
public sealed class IamIdentityDbContext : IdentityUserContext<ApplicationUser, Guid>
{
    /// <summary>Initializes a new instance of the <see cref="IamIdentityDbContext"/> class.Initializes the SQL Server Identity persistence context.</summary>
    /// <param name="options">Configured EF Core context options.</param>
    public IamIdentityDbContext(DbContextOptions<IamIdentityDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets the globally registered organizations.</summary>
    public DbSet<Tenant> Tenants => this.Set<Tenant>();

    /// <summary>Gets user-to-organization memberships.</summary>
    public DbSet<TenantMembership> TenantMemberships => this.Set<TenantMembership>();

    /// <summary>Applies Identity and IAM table mappings and relational constraints.</summary>
    /// <param name="builder">The model configuration builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("identity");

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers", "identity");
            entity.Property(user => user.Email).HasMaxLength(256);
            entity.Property(user => user.NormalizedEmail).HasMaxLength(256);
            entity.Property(user => user.UserName).HasMaxLength(256);
            entity.Property(user => user.NormalizedUserName).HasMaxLength(256);
            entity.HasIndex(user => user.NormalizedEmail)
                .IsUnique()
                .HasFilter("[NormalizedEmail] IS NOT NULL")
                .HasDatabaseName("UX_AspNetUsers_NormalizedEmail");
            entity.Property(user => user.CreatedAtUtc).HasColumnType("datetime2(7)");
            entity.Property(user => user.UpdatedAtUtc).HasColumnType("datetime2(7)");
        });

        builder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", "identity");

        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants", "identity", table =>
                table.HasCheckConstraint("CK_Tenants_Status", "[Status] IN (1, 2)"));
            entity.HasKey(tenant => tenant.Id);
            entity.Property(tenant => tenant.Name).HasMaxLength(200).IsRequired();
            entity.Property(tenant => tenant.Slug).HasMaxLength(100)
                .UseCollation("Latin1_General_100_BIN2").IsRequired();
            entity.HasIndex(tenant => tenant.Slug).IsUnique();
            entity.Property(tenant => tenant.Status).HasConversion<byte>();
            entity.Property(tenant => tenant.CreatedAtUtc).HasColumnType("datetime2(7)");
            entity.Property(tenant => tenant.UpdatedAtUtc).HasColumnType("datetime2(7)");
            entity.Property(tenant => tenant.RowVersion).IsRowVersion();
        });

        builder.Entity<TenantMembership>(entity =>
        {
            entity.ToTable("TenantMemberships", "identity", table =>
                table.HasCheckConstraint("CK_TenantMemberships_Status", "[Status] IN (1, 2)"));
            entity.HasKey(membership => new { membership.TenantId, membership.UserId });
            entity.HasOne<Tenant>().WithMany()
                .HasForeignKey(membership => membership.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(membership => membership.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(membership => new { membership.UserId, membership.Status });
            entity.Property(membership => membership.Status).HasConversion<byte>();
            entity.Property(membership => membership.CreatedAtUtc).HasColumnType("datetime2(7)");
            entity.Property(membership => membership.UpdatedAtUtc).HasColumnType("datetime2(7)");
            entity.Property(membership => membership.RowVersion).IsRowVersion();
        });
    }
}
