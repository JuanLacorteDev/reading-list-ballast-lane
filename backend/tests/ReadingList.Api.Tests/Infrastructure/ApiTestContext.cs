using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using ReadingList.Application.Auth;
using ReadingList.Books.Api;
using ReadingList.Identity.Api;

namespace ReadingList.Api.Tests.Infrastructure;

/// <summary>
/// Runs both APIs against the same temporary SQLite file, like they run for real.
/// xUnit creates a new test class instance per test, so every test gets a fresh database.
/// </summary>
public sealed class ApiTestContext : IAsyncLifetime
{
    public const string SigningKey = "test-signing-key-0123456789abcdef-0123456789";
    public const string DefaultPassword = "Str0ngPassword!";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"readinglist-api-tests-{Guid.NewGuid():N}.db");

    public ApiFactory<IdentityApiMarker> Identity { get; private set; } = null!;
    public ApiFactory<BooksApiMarker> Books { get; private set; } = null!;

    public Task InitializeAsync()
    {
        var connectionString = $"Data Source={_dbPath};Foreign Keys=True";
        Identity = new ApiFactory<IdentityApiMarker>(connectionString, SigningKey);
        Books = new ApiFactory<BooksApiMarker>(connectionString, SigningKey);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Identity.DisposeAsync();
        await Books.DisposeAsync();

        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _dbPath, $"{_dbPath}-wal", $"{_dbPath}-shm" })
            if (File.Exists(file)) File.Delete(file);
    }

    public async Task<HttpResponseMessage> RegisterAsync(string email, string password = DefaultPassword, string name = "Reader")
    {
        var client = Identity.CreateClient();
        return await client.PostAsJsonAsync("/api/auth/register", new RegisterUserRequest(name, email, password), Json);
    }

    public async Task<string> RegisterAndLoginAsync(string email = "reader@example.com")
    {
        (await RegisterAsync(email)).EnsureSuccessStatusCode();

        var client = Identity.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, DefaultPassword), Json);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(Json);
        return auth!.AccessToken;
    }

    public HttpClient BooksClient(string? token = null) => WithToken(Books.CreateClient(), token);

    public HttpClient IdentityClient(string? token = null) => WithToken(Identity.CreateClient(), token);

    private static HttpClient WithToken(HttpClient client, string? token)
    {
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
