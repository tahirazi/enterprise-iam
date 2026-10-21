// <copyright file="Program.cs" company=""EnterpriseIam">
// Copyright (c) EnterpriseIam. All rights reserved.
// </copyright>

using Duende.IdentityServer.EntityFramework.DbContexts;
using EnterpriseIam.Identity.Api.Bootstrap;
using EnterpriseIam.Identity.Api.Contracts;
using EnterpriseIam.Identity.Api.Oidc;
using EnterpriseIam.Identity.Infrastructure.DependencyInjection;
using EnterpriseIam.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddRazorPages();

// Preserve the documented EF tooling connection setting for the API host.
var sqlConnection = Environment.GetEnvironmentVariable("ENTERPRISEIAM_SQL_CONNECTION");
if (!string.IsNullOrWhiteSpace(sqlConnection))
{
    builder.Configuration["ConnectionStrings:IdentityDatabase"] = sqlConnection;
}

if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("IdentityDatabase")))
{
    builder.Services.AddIdentityPersistence(builder.Configuration);
    builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

    // IdentityServer uses ASP.NET Identity's application cookie; the future BFF gets a distinct scheme.
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.Cookie.Name = "__Host-EnterpriseIam.IdP";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
    });

    builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    {
        options.ValidationInterval = TimeSpan.Zero;
    });

    var identityServer = builder.Services.AddIdentityServer(options =>
    {
        options.UserInteraction.LoginUrl = "/account/login";
        options.UserInteraction.LogoutUrl = "/account/logout";
    })
        .AddAspNetIdentity<ApplicationUser>()
        .AddInMemoryIdentityResources(OidcClientConfiguration.IdentityResources())
        .AddInMemoryClients(OidcClientConfiguration.Clients(builder.Configuration))
        .AddOperationalStore(options =>
        {
            options.DefaultSchema = "idsrv";
            options.ConfigureDbContext = db => db.UseSqlServer(
                sqlConnection ??
                builder.Configuration.GetConnectionString("IdentityDatabase"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "idsrv");
                });
            options.EnableTokenCleanup = true;
        });

    // Development-only credentials. Do not allow IdentityServer to invent ephemeral production keys.
    if (builder.Environment.IsDevelopment())
    {
        identityServer.AddDeveloperSigningCredential(persistKey: true);
    }
    else
    {
        throw new InvalidOperationException(
            "Production signing credentials, key management and persistence must be configured before deployment.");
    }
}

var app = builder.Build();

if (args.Length > 0 && args[0] == "--bootstrap")
{
    return await DevelopmentBootstrap.RunAsync(app, args);
}

if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("IdentityDatabase")))
{
    app.UseIdentityServer();
    app.MapRazorPages();
}

app.MapHealthChecks("/health");
app.MapGet("/api/v1/system/info", () => Results.Ok(new SystemInfoResponse("Enterprise IAM", "1.0")));
app.Run();
return 0;

/// <summary>Provides the application entry point for API integration testing.</summary>
public partial class Program;
