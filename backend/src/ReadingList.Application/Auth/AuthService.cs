using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Application.Abstractions.Security;
using ReadingList.Application.Common.Results;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Application.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<Result<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
                return AuthErrors.NotFound;

            return UserResponse.FromDomain(user);
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var email = Email.Create(request.Email);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            var passwordHash = user?.PasswordHash ?? _passwordHasher.Hash("timing-attack-dummy");
            var isPasswordValid = _passwordHasher.Verify(request.Password, passwordHash);

            if (user is null || !isPasswordValid)
                return AuthErrors.InvalidCredentials;

            return AuthResponse.FromAccessToken(_tokenService.CreateToken(user));
        }

        public async Task<Result<UserResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            var email = Email.Create(request.Email);

            if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
                return AuthErrors.EmailAlreadyInUse;

            var password = Password.Create(request.Password);
            var passwordHash = _passwordHasher.Hash(password.Value);

            var user = User.Create(email, request.Name, passwordHash);
            await _userRepository.AddAsync(user, cancellationToken);

            return UserResponse.FromDomain(user);
        }
    }
}
