using ReadingList.Application.Auth;
using ReadingList.Application.Books;
using ReadingList.Application.Common.Results;
using ReadingList.Application.Tests.Fakes;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;

namespace ReadingList.Application.Tests.Books
{
    public sealed class AuthServiceTests
    {
        private static readonly Guid UserId = Guid.NewGuid();
        private static readonly Guid OtherUserId = Guid.NewGuid();

        private readonly InMemoryUserRepository _repository = new();
        private readonly AuthService _service;

        public AuthServiceTests() => _service = new AuthService(_repository);

        private User SeedUser(Guid id)
        {
            var email = Email.Create("juanlacortedev@gmail.com");
            var passwordHash = "";
            var user = User.Create(email, "Juan", passwordHash);

            _repository.AddAsync(user).GetAwaiter().GetResult();
            return user;
        }

        [Fact(DisplayName = "Shoud register a user when a valid data is provided and return it")]
        public async Task RegisterAsync_WithValidRequest_PersistsAndReturnsUser()
        {
            //Arrange
            var request = new RegisterUserRequest("Juan", "juanlacortedev@gmail.com", "");

            //Act
            var result = await _service.RegisterAsync(request);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Juan", result.Value.Name);
            Assert.Equal("juanlacortedev@gmail.com", result.Value.Email);
        }

        [Theory(DisplayName = "Should return validation error when try register with invalida data")]
        [InlineData("juan", "juanlacortedev@gmail.com", "")]
        [InlineData("juan", "notvalidemail.com", "#AcE135@Gi")]
        public async Task RegisterAsync_WithInvalidRequest_ReturnValidatoinResult(
            string name,
            string email,
            string password)
        {
            //Arrange
            var request = new RegisterUserRequest(name, email, password);

            //Act
            var result = await _service.RegisterAsync(request);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Juan", result.Value.Name);
            Assert.Equal("juanlacortedev@gmail.com", result.Value.Email);
        }

        [Fact(DisplayName = "Should return not found error when try get a not existing ID in DB")]
        public async Task GetyByIdAsync_WhenValidRequest_ReturnsUserNotFoud()
        {
            //Arrange
            var user = SeedUser(UserId);

            //Act
            var result = await _service.GetCurrentUserAsync(OtherUserId);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        }
    }
}
