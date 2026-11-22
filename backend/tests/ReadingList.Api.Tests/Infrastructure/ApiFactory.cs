using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReadingList.Api.Tests.Infrastructure;

/// <summary>
/// Hosts one of the APIs in memory, pointed at a test database and a test signing key.
/// </summary>
public sealed class ApiFactory<TMarker>(string connectionString, string signingKey) : WebApplicationFactory<TMarker>
    where TMarker : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Jwt:SigningKey", signingKey);
        builder.UseSetting("Database:SeedDemoData", "false");
    }
}
