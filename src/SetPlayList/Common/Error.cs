namespace SetPlayList.Common;

internal enum ErrorType
{
    Failure,
    NotFound,
    Validation,
    Unauthorized,
}

internal sealed record Error(string Message, ErrorType Type = ErrorType.Failure)
{
    public static Error NotFound(string message) => new(message, ErrorType.NotFound);
    public static Error Validation(string message) => new(message, ErrorType.Validation);
    public static Error Unauthorized(string message = "Unauthorized.") => new(message, ErrorType.Unauthorized);
    public static Error Failure(string message) => new(message, ErrorType.Failure);
}
