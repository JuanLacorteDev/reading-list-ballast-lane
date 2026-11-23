using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManager.IntegrationTests.Api;

public sealed class UsersEndpointTests(TaskManagerApiFactory factory) : IClassFixture<TaskManagerApiFactory>
{
    [Fact]
    public async Task CreateUser_Returns201WithGeneratedId()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/users", new { name = "Ada" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.NotEqual(Guid.Empty, body.GetProperty("id").GetGuid());
    }

    [Theory]
    [InlineData("""{ "name": "   " }""")]
    [InlineData("""{ }""")]
    public async Task CreateUser_WithBlankName_Returns400FromDomainRule(string json)
    {
        var response = await factory.CreateClient().PostAsync("/api/users",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        await response.AssertProblemAsync(400, "User.NameRequired");
    }

    [Fact]
    public void AppSettings_EnablesSqliteForeignKeys()
    {
        var contentRoot = factory.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(contentRoot, "appsettings.json")));

        var connectionString = json.RootElement.GetProperty("ConnectionStrings").GetProperty("TaskManager").GetString();

        Assert.Contains("Foreign Keys=True", connectionString);
    }
}

public sealed class CurrentUserHeaderTests(TaskManagerApiFactory factory) : IClassFixture<TaskManagerApiFactory>
{
    public static TheoryData<string, string> TaskEndpoints => new()
    {
        { "GET", "/api/tasks" },
        { "POST", "/api/tasks" },
        { "GET", $"/api/tasks/{Guid.NewGuid()}" },
        { "PUT", $"/api/tasks/{Guid.NewGuid()}" },
        { "DELETE", $"/api/tasks/{Guid.NewGuid()}" },
    };

    private static HttpRequestMessage Request(string method, string url, string? userIdHeader)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { title = "T", status = "Pending", due_date = "2026-12-31" });
        if (userIdHeader is not null)
            request.Headers.TryAddWithoutValidation("X-User-Id", userIdHeader);
        return request;
    }

    [Theory]
    [MemberData(nameof(TaskEndpoints))]
    public async Task MissingHeader_Returns400(string method, string url)
    {
        var response = await factory.CreateClient().SendAsync(Request(method, url, userIdHeader: null));

        await response.AssertProblemAsync(400, "UserContext.MissingUserId");
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("' OR '1'='1")]
    public async Task MalformedHeader_Returns400(string headerValue)
    {
        var response = await factory.CreateClient().SendAsync(Request("GET", "/api/tasks", headerValue));

        await response.AssertProblemAsync(400, "UserContext.InvalidUserId");
    }

    [Theory]
    [MemberData(nameof(TaskEndpoints))]
    public async Task UnknownUser_Returns404(string method, string url)
    {
        var response = await factory.CreateClient().SendAsync(Request(method, url, Guid.NewGuid().ToString()));

        await response.AssertProblemAsync(404, "User.NotFound");
    }
}
