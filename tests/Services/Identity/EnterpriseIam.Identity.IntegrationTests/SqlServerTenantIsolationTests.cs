// <copyright file="SqlServerTenantIsolationTests.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using EnterpriseIam.Identity.Application.Abstractions;
using EnterpriseIam.Identity.Domain.Tenants;
using EnterpriseIam.Identity.Infrastructure.DependencyInjection;
using EnterpriseIam.Identity.Infrastructure.Persistence;
using EnterpriseIam.Identity.Infrastructure.Provisioning;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EnterpriseIam.Identity.IntegrationTests;

[TestFixture]
public sealed class SqlServerTenantIsolationTests
{
    private ServiceProvider? provider;
    private string? connection;

    [OneTimeSetUp]
    public void CheckEnvironment()
    {
        this.connection = Environment.GetEnvironmentVariable("ENTERPRISEIAM_TEST_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(this.connection))
        {
            Assert.Ignore("Set ENTERPRISEIAM_TEST_SQL_CONNECTION to a disposable SQL Server database.");
        }

        var connectionBuilder = new SqlConnectionStringBuilder(this.connection);
        if (!connectionBuilder.InitialCatalog.Contains("IntegrationTests", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Fail("Integration tests require a database name containing IntegrationTests; refusing to use a non-test database.");
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:IdentityDatabase"] = this.connection }).Build();
        services.AddLogging();
        services.AddDataProtection();
        services.AddIdentityPersistence(configuration);
        this.provider = services.BuildServiceProvider();
        await using var scope = this.provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        await db.Database.MigrateAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (this.provider is not null)
        {
            await this.provider.DisposeAsync();
        }
    }

    [Test]
    public async Task TenantReader_DoesNotReturnOtherTenantsMemberships()
    {
        await using var scope = this.provider!.CreateAsyncScope();
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
        await using var scope = this.provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        db.TenantMemberships.Add(TenantMembership.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow));
        Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
    }

    [Test]
    public async Task TenantAccess_WhenTenantOrMembershipSuspended_DeniesAccess()
    {
        await using var scope = this.provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var access = scope.ServiceProvider.GetRequiredService<ITenantAccess>();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var now = DateTime.UtcNow;
        var suffix = Guid.NewGuid().ToString("N");
        var tenant = Tenant.Create("Suspend Test", "s-" + suffix, now);
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "s-" + suffix, Email = "s-" + suffix + "@example.test" };
        db.Tenants.Add(tenant);
        Assert.That((await manager.CreateAsync(user)).Succeeded, Is.True);
        var membership = TenantMembership.Create(tenant.Id, user.Id, now);
        db.TenantMemberships.Add(membership);
        await db.SaveChangesAsync();

        Assert.That(await access.HasActiveMembershipAsync(user.Id, tenant.Id, CancellationToken.None), Is.True);
        membership.Suspend(now.AddSeconds(1));
        await db.SaveChangesAsync();
        Assert.That(await access.HasActiveMembershipAsync(user.Id, tenant.Id, CancellationToken.None), Is.False);
        tenant.Suspend(now.AddSeconds(2));
        await db.SaveChangesAsync();
        Assert.That(await access.HasActiveMembershipAsync(user.Id, tenant.Id, CancellationToken.None), Is.False);
    }

    [Test]
    public async Task TenantAccess_WithDisabledGlobalUser_DeniesAccess()
    {
        await using var scope = this.provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var access = scope.ServiceProvider.GetRequiredService<ITenantAccess>();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var suffix = Guid.NewGuid().ToString("N");
        var tenant = Tenant.Create("Disabled User", "d-" + suffix, DateTime.UtcNow);
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "d-" + suffix, Email = "d-" + suffix + "@example.test" };
        db.Tenants.Add(tenant);
        Assert.That((await manager.CreateAsync(user)).Succeeded, Is.True);
        db.TenantMemberships.Add(TenantMembership.Create(tenant.Id, user.Id, DateTime.UtcNow));
        await db.SaveChangesAsync();
        user.IsDisabled = true;
        Assert.That((await manager.UpdateAsync(user)).Succeeded, Is.True);
        Assert.That(await access.HasActiveMembershipAsync(user.Id, tenant.Id, CancellationToken.None), Is.False);
        Assert.That(await access.HasActiveMembershipAsync(Guid.Empty, tenant.Id, CancellationToken.None), Is.False);
        Assert.That(await access.HasActiveMembershipAsync(user.Id, Guid.Empty, CancellationToken.None), Is.False);
    }

    [Test]
    public async Task Membership_DuplicatePair_RejectsCompositePrimaryKey()
    {
        await using var scope = this.provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var suffix = Guid.NewGuid().ToString("N");
        var tenant = Tenant.Create("Duplicate Test", "dup-" + suffix, DateTime.UtcNow);
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "dup-" + suffix, Email = "dup-" + suffix + "@example.test" };
        db.Tenants.Add(tenant);
        Assert.That((await manager.CreateAsync(user)).Succeeded, Is.True);
        db.TenantMemberships.Add(TenantMembership.Create(tenant.Id, user.Id, DateTime.UtcNow));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        db.TenantMemberships.Add(TenantMembership.Create(tenant.Id, user.Id, DateTime.UtcNow));
        Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
    }

    [Test]
    public async Task Bootstrap_WithWeakPassword_RollsBackTenantAndUser()
    {
        await using var scope = this.provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var provisioner = scope.ServiceProvider.GetRequiredService<ITenantBootstrapProvisioner>();
        var suffix = Guid.NewGuid().ToString("N");
        var slug = "rollback-" + suffix;
        var username = "rollback-" + suffix;
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await provisioner.CreateAsync("Rollback Test", slug, username, username + "@example.test", "weak", CancellationToken.None));
        db.ChangeTracker.Clear();
        Assert.That(await db.Tenants.AnyAsync(t => t.Slug == slug), Is.False);
        Assert.That(await db.Users.AnyAsync(u => u.UserName == username), Is.False);
    }

    [Test]
    public async Task Bootstrap_WithStrongPassword_CreatesLinkedRecords()
    {
        await using var scope = this.provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var provisioner = scope.ServiceProvider.GetRequiredService<ITenantBootstrapProvisioner>();
        var suffix = Guid.NewGuid().ToString("N");
        var result = await provisioner.CreateAsync(
            "Provisioned Tenant",
            "bootstrap-" + suffix,
            "bootstrap-" + suffix,
            "bootstrap-" + suffix + "@example.test",
            "ValidBootstrapPassword#123",
            CancellationToken.None);
        db.ChangeTracker.Clear();
        Assert.That(await db.Tenants.AnyAsync(t => t.Id == result.TenantId), Is.True);
        Assert.That(await db.Users.AnyAsync(u => u.Id == result.UserId), Is.True);
        Assert.That(await db.TenantMemberships.AnyAsync(m => m.TenantId == result.TenantId && m.UserId == result.UserId), Is.True);
    }

    [Test]
    public async Task Membership_ConcurrentUpdate_RejectsStaleRowVersion()
    {
        await using var scope = this.provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var suffix = Guid.NewGuid().ToString("N");
        var tenant = Tenant.Create("Concurrency Test", "c-" + suffix, DateTime.UtcNow);
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "c-" + suffix, Email = "c-" + suffix + "@example.test" };
        db.Tenants.Add(tenant);
        Assert.That((await manager.CreateAsync(user)).Succeeded, Is.True);
        db.TenantMemberships.Add(TenantMembership.Create(tenant.Id, user.Id, DateTime.UtcNow));
        await db.SaveChangesAsync();
        await using var otherScope = this.provider!.CreateAsyncScope();
        var otherDb = otherScope.ServiceProvider.GetRequiredService<IamIdentityDbContext>();
        var first = await db.TenantMemberships.SingleAsync(m => m.TenantId == tenant.Id && m.UserId == user.Id);
        var second = await otherDb.TenantMemberships.SingleAsync(m => m.TenantId == tenant.Id && m.UserId == user.Id);
        first.Suspend(DateTime.UtcNow.AddSeconds(1));
        await db.SaveChangesAsync();
        second.Suspend(DateTime.UtcNow.AddSeconds(2));
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await otherDb.SaveChangesAsync());
    }
}
