using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Application.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRespository;

        public AuthService(IUserRepository userRepository)
        {
            _userRespository = userRepository;
        }

        public Task<Result<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<UserResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
