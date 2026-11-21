using ReadingList.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Application.Auth
{
    public static class AuthErrors
    {
        public static readonly Error InvalidCredentials = Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
        public static readonly Error EmailAlreadyInUse = Error.Conflict("Auth.EmailAlreadyInUse", "Email is already registered.");
        public static readonly Error NotFound = Error.NotFound("Auth.UserNotFound", "User was not found.");
    }
}
