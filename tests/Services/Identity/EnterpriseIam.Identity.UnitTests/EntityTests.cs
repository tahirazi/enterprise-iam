using EnterpriseIam.Identity.Domain.Common;
using NUnit.Framework;

namespace EnterpriseIam.Identity.UnitTests;

[TestFixture]
public sealed class EntityTests
{
    [Test]
    public void NewEntity_ShouldHaveIdentifier()
    {
        var entity = new TestEntity();
        Assert.That(entity.Id, Is.Not.EqualTo(Guid.Empty));
    }

    private sealed class TestEntity : Entity;
}
