using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Api.Tests.Infrastructure;
using ReadingList.Application.Auth;

namespace ReadingList.Api.Tests.Identity;

public sealed class AuthEndpointsTests : IAsyncLifetime
{
    private readonly ApiTestContext _ctx = new();

    public Task InitializeAsync() => _ctx.InitializeAsync();

    public Task DisposeAsync() => _ctx.DisposeAsync();

    [Fact]
    public async Task Register_WithValidData_Returns201WithUser()
    {
        var response = await _ctx.RegisterAsync("Reader@Example.com");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>(ApiTestContext.Json);
        Assert.NotNull(user);
        Assert.Equal("reader@example.com", user.Email);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        await _ctx.RegisterAsync("reader@example.com");

        var response = await _ctx.RegisterAsync("READER@example.com");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ApiTestContext.Json);
        Assert.Equal("Auth.EmailAlreadyInUse", problem!.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task Register_WithShortPassword_Returns400()
    {
        var response = await _ctx.RegisterAsync("reader@example.com", password: "short");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var response = await _ctx.RegisterAsync("not-an-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        await _ctx.RegisterAsync("reader@example.com");

        var response = await _ctx.IdentityClient().PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("reader@example.com", ApiTestContext.DefaultPassword), ApiTestContext.Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(ApiTestContext.Json);
        Assert.False(string.IsNullOrWhiteSpace(auth!.AccessToken));
        Assert.True(auth.ExpiresAt > DateTime.UtcNow);
    }

    [Theory]
    [InlineData("reader@example.com", "WrongPassword!")]
    [InlineData("nobody@example.com", ApiTestContext.DefaultPassword)]
    public async Task Login_WithBadCredentials_Returns401WithSameError(string email, string password)
    {
        await _ctx.RegisterAsync("reader@example.com");

        var response = await _ctx.IdentityClient().PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(email, password), ApiTestContext.Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ApiTestContext.Json);
        Assert.Equal("Auth.InvalidCredentials", problem!.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _ctx.IdentityClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsCurrentUser()
    {
        var token = await _ctx.RegisterAndLoginAsync("reader@example.com");

        var response = await _ctx.IdentityClient(token).GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>(ApiTestContext.Json);
        Assert.Equal("reader@example.com", user!.Email);
    }

    [Fact]
    public async Task Me_WithTamperedToken_Returns401()
    {
        var token = await _ctx.RegisterAndLoginAsync();
        var tampered = token[..^4] + (token.EndsWith("AAAA") ? "BBBB" : "AAAA");

        var response = await _ctx.IdentityClient(tampered).GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
