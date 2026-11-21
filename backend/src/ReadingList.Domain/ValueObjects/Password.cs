using ReadingList.Domain.Common.Exceptions;
using ReadingList.Domain.Common.Validations;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Domain.ValueObjects
{
    public sealed record Password
    {
        public const int MinLength = 8;
        public const int MaxLength = 128;

        public string Value { get; }

        private Password(string value) => Value = value;

        public static Password Create(string value)
        {
            Guard.AgainstNullOrWhiteSpace(value, nameof(Password));
            Guard.Against<DomainException>(value.Length < MinLength, $"Password must have at least {MinLength} characters.");
            Guard.Against<DomainException>(value.Length > MaxLength, $"Password must have at most {MaxLength} characters.");

            return new Password(value);
        }

        public override string ToString() => "********";
    }
}
