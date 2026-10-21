namespace EnterpriseIam.Identity.Domain.Common;

/// <summary>
/// Provides a unique identifier for identity domain entities.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Gets the stable unique identifier of the entity.
    /// </summary>
    public Guid Id
    {
        get;
        protected init;
    } = Guid.NewGuid();
}
