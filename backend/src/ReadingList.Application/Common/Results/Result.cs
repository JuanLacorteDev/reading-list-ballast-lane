namespace ReadingList.Application.Common.Results;

/// <summary>
/// Outcome of a use case without a return value. Expected failures are returned, not thrown.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Outcome of a use case that returns a value on success.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, null) => _value = value;

    private Result(Error error) : base(false, error) => _value = default;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
