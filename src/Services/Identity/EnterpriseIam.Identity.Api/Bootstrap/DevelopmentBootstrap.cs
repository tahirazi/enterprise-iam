// <copyright file="DevelopmentBootstrap.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using EnterpriseIam.Identity.Infrastructure.Persistence;
using EnterpriseIam.Identity.Infrastructure.Provisioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIam.Identity.Api.Bootstrap;

/// <summary>
/// Runs an explicitly enabled, development-only initial-tenant provisioning command.
/// </summary>
internal static class DevelopmentBootstrap
{
    /// <summary>
    /// Validates bootstrap arguments and provisions a tenant with its initial user.
    /// </summary>
    /// <param name="app">The configured application and service provider.</param>
    /// <param name="args">The command-line arguments provided to the application.</param>
    /// <returns>A process exit code indicating success or failure.</returns>
    internal static async Task<int> RunAsync(WebApplication app, string[] args)
    {
        if (!app.Environment.IsDevelopment() ||
            !string.Equals(Environment.GetEnvironmentVariable("ENTERPRISEIAM_ENABLE_BOOTSTRAP"), "true", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Bootstrap is disabled. Development environment and ENTERPRISEIAM_ENABLE_BOOTSTRAP=true are required.");
            return 1;
        }

        if (args.Length != 5 || args[0] != "--bootstrap")
        {
            Console.Error.WriteLine("Usage: --bootstrap <tenant-name> <tenant-slug> <username> <email>");
            return 1;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var provisioner = scope.ServiceProvider.GetRequiredService<ITenantBootstrapProvisioner>();

        if (!await database.Database.CanConnectAsync())
        {
            Console.Error.WriteLine("Database is not reachable; apply the reviewed EF Core migration first.");
            return 1;
        }

        var slug = args[2];
        var username = args[3];
        var email = args[4];
        var existingTenant = await database.Tenants.AsNoTracking().SingleOrDefaultAsync(x => x.Slug == slug);
        var existingUser = await users.FindByNameAsync(username);
        var existingEmailUser = await users.FindByEmailAsync(email);

        if (existingTenant is not null || existingUser is not null || existingEmailUser is not null)
        {
            Console.Error.WriteLine("Tenant slug, username, or email already exists. No changes made. Inspect existing records before retrying.");
            return 2;
        }

        Console.Write("Initial password (input hidden): ");
        var password = ReadSecret();
        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.Error.WriteLine("Password is required.");
            return 1;
        }

        try
        {
            var result = await provisioner.CreateAsync(args[1], slug, username, email, password, CancellationToken.None);
            Console.WriteLine($"Tenant created: {result.TenantId}");
            Console.WriteLine($"Initial user created: {result.UserId}");
            Console.WriteLine("The account is not yet a tenant administrator; roles and permissions are a future milestone.");
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or DbUpdateException)
        {
            Console.Error.WriteLine($"Bootstrap failed: {exception.GetType().Name}. Check input, database constraints, and application logs.");
            return 1;
        }
    }

    /// <summary>
    /// Reads a secret from an interactive terminal without echoing characters.
    /// </summary>
    /// <returns>The secret entered by the operator.</returns>
    private static string ReadSecret()
    {
        var characters = new System.Text.StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (characters.Length > 0)
                {
                    characters.Length--;
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                characters.Append(key.KeyChar);
            }
        }

        return characters.ToString();
    }
}
