// <copyright file="MembershipStatus.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

namespace EnterpriseIam.Identity.Domain.Tenants;

/// <summary>Describes the current eligibility of an identity within a tenant.</summary>
public enum MembershipStatus : byte
{
    /// <summary>The identity is eligible for tenant authorization.</summary>
    Active = 1,

    /// <summary>The identity is not eligible for tenant authorization.</summary>
    Suspended = 2,
}
