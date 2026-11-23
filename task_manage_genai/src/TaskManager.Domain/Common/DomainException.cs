namespace TaskManager.Domain.Common;

/// <summary>
/// Thrown when a business rule is broken. Mapped to HTTP 400 by the API's global
/// DomainExceptionHandler; application services never catch it.
/// </summary>
public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
