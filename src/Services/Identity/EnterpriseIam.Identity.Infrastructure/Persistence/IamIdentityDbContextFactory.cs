using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseIam.Identity.Infrastructure.Persistence;

/// <summary>Creates design-time contexts for controlled EF Core SQL Server migrations.</summary>
public sealed class IamIdentityDbContextFactory : IDesignTimeDbContextFactory<IamIdentityDbContext>
{
    /// <summary>Creates the migrations design-time context without relying on HTTP startup.</summary>
    /// <param name="args">Arguments supplied by EF tooling.</param>
    /// <returns>A configured SQL Server identity context.</returns>
    /// <exception cref="InvalidOperationException">No connection string is configured.</exception>
    public IamIdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ENTERPRISEIAM_SQL_CONNECTION")
            ?? throw new InvalidOperationException("Set ENTERPRISEIAM_SQL_CONNECTION before running EF migrations.");
        var options = new DbContextOptionsBuilder<IamIdentityDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity"))
            .Options;
        return new IamIdentityDbContext(options);
    }
}
