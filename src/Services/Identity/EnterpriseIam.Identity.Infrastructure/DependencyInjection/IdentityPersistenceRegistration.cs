// <copyright file="IdentityPersistenceRegistration.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using EnterpriseIam.Identity.Application.Abstractions;
using EnterpriseIam.Identity.Infrastructure.Persistence;
using EnterpriseIam.Identity.Infrastructure.Provisioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseIam.Identity.Infrastructure.DependencyInjection;

/// <summary>Registers SQL Server persistence and ASP.NET Identity account services.</summary>
public static class IdentityPersistenceRegistration
{
    /// <summary>Adds the Identity persistence boundary to a dependency-injection container.</summary>
    /// <param name="services">The host's service collection.</param>
    /// <param name="configuration">The host's application configuration.</param>
    /// <returns>The service collection for further registration.</returns>
    /// <exception cref="InvalidOperationException">The SQL connection string is missing.</exception>
    public static IServiceCollection AddIdentityPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("IdentityDatabase")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:IdentityDatabase.");

        services.AddDbContext<IamIdentityDbContext>(options =>
            options.UseSqlServer(connection, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
        }).AddEntityFrameworkStores<IamIdentityDbContext>();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITenantAccess, TenantAccess>();
        services.AddScoped<ITenantMemberReader, TenantMemberReader>();
        services.AddScoped<ITenantBootstrapProvisioner, TenantBootstrapProvisioner>();
        return services;
    }
}
