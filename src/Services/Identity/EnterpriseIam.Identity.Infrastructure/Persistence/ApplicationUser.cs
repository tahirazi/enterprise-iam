// <copyright file="ApplicationUser.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Identity;

namespace EnterpriseIam.Identity.Infrastructure.Persistence;

/// <summary>Stores global authentication credentials and framework-managed account state.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Gets or sets a value indicating whether gets or sets whether all account access is administratively disabled.</summary>
    public bool IsDisabled
    {
        get; set;
    }

    /// <summary>Gets or sets the UTC time at which the account was created.</summary>
    public DateTime CreatedAtUtc
    {
        get; set;
    }

    /// <summary>Gets or sets the UTC time at which the account was last modified.</summary>
    public DateTime UpdatedAtUtc
    {
        get; set;
    }
}
