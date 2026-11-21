using ReadingList.Domain.Commom.Exceptions;
using ReadingList.Domain.Commom.Validations;

namespace ReadingList.Domain.ValueObjects
{
    public sealed record Email
    {
        public string Value { get; }

        private Email(string value) => Value = value;

        public static Email Create(string value)
        {
            Guard.AgainstNullOrWhiteSpace(value, nameof(value));

            var normalized = value.Trim().ToLowerInvariant();

            Guard.Against<DomainException>(!IsValid(normalized), "Email is not valid.");

            return new Email(normalized);
        }

        private static bool IsValid(string value)
        {
            if (value.Length > 254) return false;

            return System.Net.Mail.MailAddress.TryCreate(value, out var addr) && addr.Address == value;
        }

        public override string ToString() => Value;
    }
}
