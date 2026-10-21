namespace EnterpriseIam.Identity.Api.Contracts;

/// <summary>
/// Describes public version information about the Enterprise IAM API.
/// </summary>
/// <param name="Application">The product name exposed by the API.</param>
/// <param name="ArchitectureVersion">The current architectural baseline.</param>
public sealed record SystemInfoResponse(string Application, string ArchitectureVersion);
