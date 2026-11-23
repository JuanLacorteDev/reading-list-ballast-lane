using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Tasks;

namespace TaskManager.IntegrationTests.Api;

public sealed class TasksEndpointTests(TaskManagerApiFactory factory) : IClassFixture<TaskManagerApiFactory>
{
    private static object NewTask(string title = "Write report", string? status = "InProgress", string? dueDate = "2026-12-31")
        => new { title, description = "Quarterly", status, due_date = dueDate };

    private async Task<(HttpClient Client, Guid UserId)> NewUserClientAsync()
    {
        var userId = await factory.CreateUserAsync();
        return (factory.CreateClientFor(userId), userId);
    }

    private static async Task<Guid> CreateTaskAsync(HttpClient client, object? body = null)
    {
        var response = await client.PostAsJsonAsync("/api/tasks", body ?? NewTask());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.ReadJsonAsync()).GetProperty("id").GetGuid();
    }

    private static StringContent Json(string raw) => new(raw, Encoding.UTF8, "application/json");

    // ---------- CRUD happy paths ----------

    [Fact]
    public async Task Create_Returns201_WithLocationAndSnakeCaseBody()
    {
        var (client, _) = await NewUserClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", NewTask());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.ReadJsonAsync();
        var id = body.GetProperty("id").GetGuid();
        Assert.Equal($"/api/tasks/{id}", response.Headers.Location?.AbsolutePath);
        Assert.Equal("Write report", body.GetProperty("title").GetString());
        Assert.Equal("Quarterly", body.GetProperty("description").GetString());
        Assert.Equal("InProgress", body.GetProperty("status").GetString());
        Assert.Equal("2026-12-31", body.GetProperty("due_date").GetString());
        Assert.False(body.TryGetProperty("dueDate", out _));
    }

    [Fact]
    public async Task Create_WithoutStatus_DefaultsToPending()
    {
        var (client, _) = await NewUserClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", new { title = "T", due_date = "2026-12-31" });

        Assert.Equal("Pending", (await response.ReadJsonAsync()).GetProperty("status").GetString());
    }

    [Fact]
    public async Task Create_IgnoresUserIdInBody_OwnerComesFromHeader()
    {
        var (client, userId) = await NewUserClientAsync();
        var (otherClient, otherUserId) = await NewUserClientAsync();

        var id = await CreateTaskAsync(client,
            new { title = "Mine", status = "Pending", due_date = "2026-12-31", user_id = otherUserId });

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/tasks/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await otherClient.GetAsync($"/api/tasks/{id}")).StatusCode);
        await using var connection = new SqliteConnection(factory.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT user_id FROM tasks WHERE id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());
        Assert.Equal(userId.ToString(), await command.ExecuteScalarAsync());
    }

    [Fact]
    public async Task List_ReturnsOnlyCurrentUsersTasks()
    {
        var (client, _) = await NewUserClientAsync();
        var (otherClient, _) = await NewUserClientAsync();
        var mine = await CreateTaskAsync(client);
        await CreateTaskAsync(otherClient);

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = (await response.ReadJsonAsync()).EnumerateArray().ToList();
        var only = Assert.Single(items);
        Assert.Equal(mine, only.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task List_ForUserWithoutTasks_ReturnsEmptyArray()
    {
        var (client, _) = await NewUserClientAsync();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.ReadJsonAsync()).EnumerateArray());
    }

    [Fact]
    public async Task Get_OwnTask_Returns200()
    {
        var (client, _) = await NewUserClientAsync();
        var id = await CreateTaskAsync(client);

        var response = await client.GetAsync($"/api/tasks/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadJsonAsync();
        Assert.Equal(id, body.GetProperty("id").GetGuid());
        Assert.Equal("InProgress", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Update_OwnTask_Returns200AndPersists()
    {
        var (client, _) = await NewUserClientAsync();
        var id = await CreateTaskAsync(client);

        var response = await client.PutAsJsonAsync($"/api/tasks/{id}",
            new { title = "Updated", description = (string?)null, status = "Completed", due_date = "2027-01-15" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reloaded = await (await client.GetAsync($"/api/tasks/{id}")).ReadJsonAsync();
        Assert.Equal("Updated", reloaded.GetProperty("title").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, reloaded.GetProperty("description").ValueKind);
        Assert.Equal("Completed", reloaded.GetProperty("status").GetString());
        Assert.Equal("2027-01-15", reloaded.GetProperty("due_date").GetString());
    }

    [Fact]
    public async Task Delete_OwnTask_Returns204AndRemovesIt()
    {
        var (client, _) = await NewUserClientAsync();
        var id = await CreateTaskAsync(client);

        var response = await client.DeleteAsync($"/api/tasks/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await (await client.GetAsync($"/api/tasks/{id}")).AssertProblemAsync(404, "Task.NotFound");
    }

    // ---------- Ownership ----------

    [Fact]
    public async Task OtherUsersTask_GetUpdateDelete_Return404_AndTaskIsUntouched()
    {
        var (owner, _) = await NewUserClientAsync();
        var (intruder, _) = await NewUserClientAsync();
        var id = await CreateTaskAsync(owner);

        await (await intruder.GetAsync($"/api/tasks/{id}")).AssertProblemAsync(404, "Task.NotFound");
        await (await intruder.PutAsJsonAsync($"/api/tasks/{id}",
            new { title = "Hijacked", status = "Completed", due_date = "2030-01-01" })).AssertProblemAsync(404, "Task.NotFound");
        await (await intruder.DeleteAsync($"/api/tasks/{id}")).AssertProblemAsync(404, "Task.NotFound");

        var body = await (await owner.GetAsync($"/api/tasks/{id}")).ReadJsonAsync();
        Assert.Equal("Write report", body.GetProperty("title").GetString());
        Assert.Equal("InProgress", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task OtherUsersTask_And_MissingTask_ReturnIdenticalResponses()
    {
        var (owner, _) = await NewUserClientAsync();
        var (intruder, _) = await NewUserClientAsync();
        var id = await CreateTaskAsync(owner);

        var foreign = await (await intruder.GetAsync($"/api/tasks/{id}")).ReadJsonAsync();
        var missing = await (await intruder.GetAsync($"/api/tasks/{Guid.NewGuid()}")).ReadJsonAsync();

        Assert.Equal(missing.GetProperty("code").GetString(), foreign.GetProperty("code").GetString());
        Assert.Equal(missing.GetProperty("detail").GetString(), foreign.GetProperty("detail").GetString());
    }

    // ---------- Validation ----------

    [Theory]
    [InlineData("""{ "title": "  ", "due_date": "2026-12-31" }""")]
    [InlineData("""{ "due_date": "2026-12-31" }""")]
    public async Task Create_WithBlankOrMissingTitle_Returns400(string json)
    {
        var (client, _) = await NewUserClientAsync();

        var response = await client.PostAsync("/api/tasks", Json(json));

        await response.AssertProblemAsync(400, "Task.TitleRequired");
    }

    [Theory]
    [InlineData("Done")]
    [InlineData("1")]
    public async Task Create_WithUnsupportedStatus_Returns400(string status)
    {
        var (client, _) = await NewUserClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", NewTask(status: status));

        await response.AssertProblemAsync(400, "Task.InvalidStatus");
    }

    [Theory]
    [InlineData("31/12/2026")]
    [InlineData("2026-13-01")]
    [InlineData("2026-02-30")]
    [InlineData("2026-12-31T10:00:00Z")]
    [InlineData("not a date")]
    public async Task Create_WithInvalidDueDate_Returns400(string dueDate)
    {
        var (client, _) = await NewUserClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", NewTask(dueDate: dueDate));

        await response.AssertProblemAsync(400, "Task.InvalidDueDate");
    }

    [Fact]
    public async Task Create_WithoutDueDate_Returns400()
    {
        var (client, _) = await NewUserClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks", NewTask(dueDate: null));

        await response.AssertProblemAsync(400, "Task.DueDateRequired");
    }

    [Fact]
    public async Task Update_WithoutStatus_Returns400()
    {
        var (client, _) = await NewUserClientAsync();
        var id = await CreateTaskAsync(client);

        var response = await client.PutAsJsonAsync($"/api/tasks/{id}", new { title = "T", due_date = "2026-12-31" });

        await response.AssertProblemAsync(400, "Task.StatusRequired");
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("""{ "title": 42, "due_date": "2026-12-31" }""")]
    [InlineData("""{ "title": "T", "status": 1, "due_date": "2026-12-31" }""")]
    [InlineData("")]
    public async Task Create_WithMalformedBody_Returns400(string raw)
    {
        var (client, _) = await NewUserClientAsync();

        var response = await client.PostAsync("/api/tasks", Json(raw));

        await response.AssertProblemAsync(400, "Request.Invalid");
    }

    // ---------- DateOnly & SQL injection ----------

    [Theory]
    [InlineData("1999-01-01")]
    [InlineData("2024-02-29")]
    [InlineData("2099-12-31")]
    public async Task DueDate_RoundTripsAsDateOnlyString_IncludingPastDates(string dueDate)
    {
        var (client, _) = await NewUserClientAsync();
        var id = await CreateTaskAsync(client, NewTask(dueDate: dueDate));

        var body = await (await client.GetAsync($"/api/tasks/{id}")).ReadJsonAsync();

        Assert.Equal(dueDate, body.GetProperty("due_date").GetString());
    }

    [Theory]
    [InlineData("'); DROP TABLE tasks; --")]
    [InlineData("x' OR '1'='1")]
    public async Task SqlInjectionLikeTitle_IsTreatedAsData(string payload)
    {
        var (client, _) = await NewUserClientAsync();
        var (otherClient, _) = await NewUserClientAsync();
        await CreateTaskAsync(otherClient);

        var id = await CreateTaskAsync(client, NewTask(title: payload));

        var body = await (await client.GetAsync($"/api/tasks/{id}")).ReadJsonAsync();
        Assert.Equal(payload, body.GetProperty("title").GetString());
        Assert.Single((await (await client.GetAsync("/api/tasks")).ReadJsonAsync()).EnumerateArray());
        Assert.Single((await (await otherClient.GetAsync("/api/tasks")).ReadJsonAsync()).EnumerateArray());
    }

    // ---------- Unexpected failures ----------

    [Fact]
    public async Task UnexpectedException_Returns500_WithoutInternalDetails()
    {
        var userId = await factory.CreateUserAsync();
        var client = factory
            .WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddScoped<ITaskRepository, ThrowingTaskRepository>()))
            .CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(ThrowingTaskRepository.SecretMessage, raw);
        Assert.DoesNotContain("StackTrace", raw, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ThrowingTaskRepository : ITaskRepository
    {
        public const string SecretMessage = "connection string secret-value leaked";
        private static Exception Boom() => new InvalidOperationException(SecretMessage);

        public Task AddAsync(TaskItem task, CancellationToken ct = default) => throw Boom();
        public Task<TaskItem?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken ct = default) => throw Boom();
        public Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken ct = default) => throw Boom();
        public Task<bool> UpdateAsync(TaskItem task, CancellationToken ct = default) => throw Boom();
        public Task<bool> DeleteAsync(Guid taskId, Guid userId, CancellationToken ct = default) => throw Boom();
    }
}
