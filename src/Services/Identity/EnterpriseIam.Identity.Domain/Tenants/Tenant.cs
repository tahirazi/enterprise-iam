// <copyright file="Tenant.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

namespace EnterpriseIam.Identity.Domain.Tenants;

/// <summary>
/// Represents an organization that owns tenant-specific memberships and resources.
/// </summary>
public sealed class Tenant
{
    private Tenant()
    {
    }

    private Tenant(Guid id, string name, string slug, DateTime utcNow)
    {
        this.Id = id;
        this.Name = name;
        this.Slug = slug;
        this.Status = TenantStatus.Active;
        this.CreatedAtUtc = utcNow;
        this.UpdatedAtUtc = utcNow;
    }

    /// <summary>Gets the immutable tenant identifier.</summary>
    public Guid Id
    {
        get; private set;
    }

    /// <summary>Gets the tenant's human-readable name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the canonical immutable routing identifier.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Gets whether tenant-scoped access is permitted.</summary>
    public TenantStatus Status
    {
        get; private set;
    }

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc
    {
        get; private set;
    }

    /// <summary>Gets the UTC last-modified timestamp.</summary>
    public DateTime UpdatedAtUtc
    {
        get; private set;
    }

    /// <summary>Gets the SQL Server concurrency token.</summary>
    public byte[] RowVersion { get; private set; } = [];

    /// <summary>Creates an active tenant with a canonical routing slug.</summary>
    /// <param name="name">The visible organization name.</param>
    /// <param name="slug">The lowercase ASCII tenant routing identifier.</param>
    /// <param name="utcNow">The current UTC timestamp.</param>
    /// <returns>The newly initialized tenant.</returns>
    /// <exception cref="ArgumentException">The name or slug is invalid.</exception>
    public static Tenant Create(string name, string slug, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
        {
            throw new ArgumentException("Tenant name must contain 1-200 characters.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 100 ||
            !System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            throw new ArgumentException("Tenant slug must be lowercase letters, numbers and internal hyphens.", nameof(slug));
        }

        if (utcNow.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("UTC timestamp required.", nameof(utcNow));
        }

        return new Tenant(Guid.NewGuid(), name.Trim(), slug, utcNow);
    }

    /// <summary>Suspends tenant-scoped access for the organization.</summary>
    /// <param name="utcNow">The current UTC timestamp.</param>
    public void Suspend(DateTime utcNow)
    {
        ValidateUtc(utcNow);
        this.Status = TenantStatus.Suspended;
        this.UpdatedAtUtc = utcNow;
    }

    /// <summary>Restores access for an eligible suspended organization.</summary>
    /// <param name="utcNow">The current UTC timestamp.</param>
    public void Activate(DateTime utcNow)
    {
        ValidateUtc(utcNow);
        this.Status = TenantStatus.Active;
        this.UpdatedAtUtc = utcNow;
    }

    private static void ValidateUtc(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("UTC timestamp required.", nameof(utcNow));
        }
    }
}
