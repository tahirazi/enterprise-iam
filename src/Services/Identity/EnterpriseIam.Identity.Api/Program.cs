using EnterpriseIam.Identity.Api.Bootstrap;
using EnterpriseIam.Identity.Api.Contracts;
using EnterpriseIam.Identity.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

// Preserve the documented EF tooling connection setting for the API host.
var sqlConnection = Environment.GetEnvironmentVariable("ENTERPRISEIAM_SQL_CONNECTION");
if (!string.IsNullOrWhiteSpace(sqlConnection))
{
    builder.Configuration["ConnectionStrings:IdentityDatabase"] = sqlConnection;
}

if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("IdentityDatabase")))
{
    builder.Services.AddIdentityPersistence(builder.Configuration);
}

var app = builder.Build();

if (args.Length > 0 && args[0] == "--bootstrap")
{
    return await DevelopmentBootstrap.RunAsync(app, args);
}

app.MapHealthChecks("/health");
app.MapGet("/api/v1/system/info", () => Results.Ok(new SystemInfoResponse("Enterprise IAM", "1.0")));
app.Run();
return 0;

/// <summary>Provides the application entry point for API integration testing.</summary>
public partial class Program;
