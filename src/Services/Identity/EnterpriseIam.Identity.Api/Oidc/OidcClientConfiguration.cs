// <copyright file="OidcClientConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;

namespace EnterpriseIam.Identity.Api.Oidc;

/// <summary>Provides explicitly registered development-only OpenID Connect clients and resources.</summary>
internal static class OidcClientConfiguration
{
    /// <summary>Creates the standard identity resources required by the first protocol test client.</summary>
    /// <returns>The enabled identity resources.</returns>
    public static IEnumerable<IdentityResource> IdentityResources()
    {
        return [new IdentityResources.OpenId(), new IdentityResources.Profile()];
    }

    /// <summary>Returns the tightly scoped development web client, if fully configured.</summary>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The registered clients.</returns>
    public static IEnumerable<Client> Clients(IConfiguration configuration)
    {
        var secret = configuration["Oidc:DevelopmentClient:Secret"];
        var redirectUri = configuration["Oidc:DevelopmentClient:RedirectUri"];
        var logoutUri = configuration["Oidc:DevelopmentClient:PostLogoutRedirectUri"];
        if (string.IsNullOrWhiteSpace(secret) ||
            !Uri.TryCreate(redirectUri, UriKind.Absolute, out var redirect) || redirect.Scheme != Uri.UriSchemeHttps ||
            !Uri.TryCreate(logoutUri, UriKind.Absolute, out var logout) || logout.Scheme != Uri.UriSchemeHttps)
        {
            return [];
        }

        return
        [
            new Client
            {
                ClientId = "enterprise-iam-oidc-test",
                ClientName = "Enterprise IAM development OIDC test client",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                AllowPlainTextPkce = false,
                RequireClientSecret = true,
                ClientSecrets = { new Secret(secret.Sha256()) },
                RedirectUris = { redirect.AbsoluteUri },
                PostLogoutRedirectUris = { logout.AbsoluteUri },
                AllowedScopes = { "openid", "profile" },
                AllowOfflineAccess = false,
                RequireConsent = false,
            },
        ];
    }
}
