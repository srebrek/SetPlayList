namespace SetPlayList.Common;

internal sealed class Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
        Error = null;
    }

    private Result(Error error)
    {
        _value = default;
        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
}
