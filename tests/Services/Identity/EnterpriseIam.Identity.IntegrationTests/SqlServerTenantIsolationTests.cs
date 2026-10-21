using EnterpriseIam.Identity.Application.Abstractions;
using EnterpriseIam.Identity.Domain.Tenants;
using EnterpriseIam.Identity.Infrastructure.DependencyInjection;
using EnterpriseIam.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EnterpriseIam.Identity.IntegrationTests;

[TestFixture]
public sealed class SqlServerTenantIsolationTests
{
    private ServiceProvider? _provider;
    private string? _connection;

    [OneTimeSetUp]
    public void CheckEnvironment()
    {
        _connection = Environment.GetEnvironmentVariable("ENTERPRISEIAM_TEST_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(_connection))
        {
            Assert.Ignore("Real SQL Server required: set ENTERPRISEIAM_TEST_SQL_CONNECTION to a disposable test database.");
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:IdentityDatabase"] = _connection }).Build();
        services.AddLogging();
        services.AddIdentityPersistence(configuration);
        _provider = services.BuildServiceProvider();
        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        await db.Database.MigrateAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_provider is not null)
        {
            await _provider.DisposeAsync();
        }
    }

    [Test]
    public async Task TenantReader_DoesNotReturnOtherTenantsMemberships()
    {
        await using var scope = _provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var reader = scope.ServiceProvider.GetRequiredService<ITenantMemberReader>();
        var now = DateTime.UtcNow;
        var suffix = Guid.NewGuid().ToString("N");
        var tenantA = Tenant.Create("Tenant A", "a-" + suffix, now);
        var tenantB = Tenant.Create("Tenant B", "b-" + suffix, now);
        db.Tenants.AddRange(tenantA, tenantB);
        var userA = new ApplicationUser { Id = Guid.NewGuid(), UserName = "usera-" + suffix, Email = "a-" + suffix + "@example.test" };
        var userB = new ApplicationUser { Id = Guid.NewGuid(), UserName = "userb-" + suffix, Email = "b-" + suffix + "@example.test" };
        Assert.That((await manager.CreateAsync(userA)).Succeeded, Is.True);
        Assert.That((await manager.CreateAsync(userB)).Succeeded, Is.True);
        db.TenantMemberships.AddRange(
            TenantMembership.Create(tenantA.Id, userA.Id, now),
            TenantMembership.Create(tenantB.Id, userB.Id, now));
        await db.SaveChangesAsync();

        var visible = await reader.GetMemberIdsAsync(userA.Id, tenantA.Id, CancellationToken.None);
        Assert.That(visible, Is.EquivalentTo(new[] { userA.Id }));
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await reader.GetMemberIdsAsync(userA.Id, tenantB.Id, CancellationToken.None));
    }

    [Test]
    public async Task Membership_WithUnknownTenant_RejectsForeignKey()
    {
        await using var scope = _provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        db.TenantMemberships.Add(TenantMembership.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow));
        Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
    }
}
