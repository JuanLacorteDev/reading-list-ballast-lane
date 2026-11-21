using ReadingList.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Application.Auth
{
    public static class AuthErrors
    {
        public static readonly Error NotFound = Error.NotFound("User.NotFound", "User was not found.");
        public static readonly Error Unauthorized = Error.Validation("User.Validation", "Email or Password is not valid");
    }
}
