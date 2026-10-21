// <copyright file="ITenantAccess.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

namespace EnterpriseIam.Identity.Application.Abstractions;

/// <summary>Provides fail-closed checks for an authenticated user's active tenant membership.</summary>
public interface ITenantAccess
{
    /// <summary>Verifies that an active account belongs to an active tenant.</summary>
    /// <param name="userId">The authenticated global user identifier.</param>
    /// <param name="tenantId">The requested tenant identifier.</param>
    /// <param name="cancellationToken">Cancels the database query.</param>
    /// <returns>True only when all eligibility checks pass.</returns>
    Task<bool> HasActiveMembershipAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken);
}
