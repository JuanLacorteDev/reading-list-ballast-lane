using ReadingList.Application.Auth;
using ReadingList.Application.Common.Results;
using ReadingList.Application.Tests.Fakes;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;

namespace ReadingList.Application.Tests.Auth;

public sealed class AuthServiceTests
{
    private const string ValidName = "Reader";
    private const string ValidEmail = "reader@example.com";
    private const string ValidPassword = "Secret123!";

    private readonly InMemoryUserRepository _repository = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeTokenService _tokenService = new();
    private readonly AuthService _service;

    public AuthServiceTests() => _service = new AuthService(_repository, _passwordHasher, _tokenService);

    private User SeedUser()
    {
        var user = User.Create(Email.Create(ValidEmail), ValidName, _passwordHasher.Hash(ValidPassword));
        _repository.AddAsync(user).GetAwaiter().GetResult();
        return user;
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_PersistsUserWithHashedPassword()
    {
        var request = new RegisterUserRequest(ValidName, ValidEmail, ValidPassword);

        var result = await _service.RegisterAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidName, result.Value.Name);
        Assert.Equal(ValidEmail, result.Value.Email);

        var saved = Assert.Single(_repository.Users);
        Assert.Equal(result.Value.Id, saved.Id);
        Assert.NotEqual(ValidPassword, saved.PasswordHash);
        Assert.Equal(_passwordHasher.Hash(ValidPassword), saved.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsConflict()
    {
        SeedUser();
        var request = new RegisterUserRequest("Another Reader", ValidEmail, "Another123!");

        var result = await _service.RegisterAsync(request);

        Assert.Equal(AuthErrors.EmailAlreadyInUse, result.Error);
        Assert.Single(_repository.Users);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmailInDifferentCase_ReturnsConflict()
    {
        SeedUser();
        var request = new RegisterUserRequest("Another Reader", ValidEmail.ToUpperInvariant(), "Another123!");

        var result = await _service.RegisterAsync(request);

        Assert.Equal(AuthErrors.EmailAlreadyInUse, result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var user = SeedUser();

        var result = await _service.LoginAsync(new LoginRequest(ValidEmail, ValidPassword));

        Assert.True(result.IsSuccess);
        Assert.Equal($"token-for-{user.Id}", result.Value.AccessToken);
        Assert.Equal(FakeTokenService.FixedExpiresAt, result.Value.ExpiresAt);
        Assert.Same(user, _tokenService.LastUser);
    }

    [Fact]
    public async Task LoginAsync_WithUppercaseEmail_Succeeds()
    {
        SeedUser();

        var result = await _service.LoginAsync(new LoginRequest(ValidEmail.ToUpperInvariant(), ValidPassword));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsInvalidCredentials()
    {
        SeedUser();

        var result = await _service.LoginAsync(new LoginRequest(ValidEmail, "WrongPass1!"));

        Assert.Equal(AuthErrors.InvalidCredentials, result.Error);
        Assert.Null(_tokenService.LastUser);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ReturnsSameErrorAsWrongPassword()
    {
        SeedUser();

        var result = await _service.LoginAsync(new LoginRequest("unknown@example.com", ValidPassword));

        Assert.Equal(AuthErrors.InvalidCredentials, result.Error);
        Assert.Null(_tokenService.LastUser);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserExists_ReturnsUser()
    {
        var user = SeedUser();

        var result = await _service.GetCurrentUserAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.Id);
        Assert.Equal(ValidEmail, result.Value.Email);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenUserDoesNotExist_ReturnsNotFound()
    {
        SeedUser();

        var result = await _service.GetCurrentUserAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
