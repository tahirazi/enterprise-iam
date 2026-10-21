// <copyright file="HealthEndpointTests.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace EnterpriseIam.Identity.IntegrationTests;

[TestFixture]
public sealed class HealthEndpointTests
{
    [Test]
    public async Task Health_ShouldReturnSuccess()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health");

        Assert.That(response.IsSuccessStatusCode, Is.True);
    }
}
