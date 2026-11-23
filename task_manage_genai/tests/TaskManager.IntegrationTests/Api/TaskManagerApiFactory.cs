using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace TaskManager.IntegrationTests.Api;

/// <summary>Boots the real API against a throwaway SQLite file database.</summary>
public sealed class TaskManagerApiFactory : WebApplicationFactory<Program>
{
    public string DatabasePath { get; } = Path.Combine(Path.GetTempPath(), $"taskmanager-api-tests-{Guid.NewGuid():N}.db");
    public string ConnectionString => $"Data Source={DatabasePath};Foreign Keys=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseSetting("ConnectionStrings:TaskManager", ConnectionString);

    public HttpClient CreateClientFor(Guid userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    public async Task<Guid> CreateUserAsync(string name = "Test user")
    {
        var response = await CreateClient().PostAsJsonAsync("/api/users", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        SqliteConnection.ClearAllPools();
        if (File.Exists(DatabasePath)) File.Delete(DatabasePath);
    }
}

internal static class HttpTestExtensions
{
    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<JsonElement>();

    /// <summary>Asserts a ProblemDetails response with the given status and error code.</summary>
    public static async Task AssertProblemAsync(this HttpResponseMessage response, int status, string code)
    {
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.ReadJsonAsync();
        Assert.Equal(status, body.GetProperty("status").GetInt32());
        Assert.Equal(code, body.GetProperty("code").GetString());
        Assert.Equal(code, body.GetProperty("title").GetString());
    }
}
