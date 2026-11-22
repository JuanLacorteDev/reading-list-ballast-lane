namespace ReadingList.Infrastructure.Security
{
    /// <summary>
    /// JWT settings bound from the "Jwt" configuration section.
    /// Both APIs must use the same Issuer, Audience and SigningKey.
    /// </summary>
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        /// <summary>HMAC-SHA256 key. Must be at least 32 bytes (256 bits).</summary>
        public string SigningKey { get; init; } = string.Empty;

        public int ExpirationMinutes { get; init; } = 60;
    }
}
