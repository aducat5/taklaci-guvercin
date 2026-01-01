namespace TaklaciGuvercin.Shared.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; protected set; } = string.Empty;
    public List<string> Errors { get; protected set; } = new();

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
        if (!string.IsNullOrEmpty(error))
            Errors.Add(error);
    }

    protected Result(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<string>();
        Error = errors?.FirstOrDefault() ?? string.Empty;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(List<string> errors) => new(false, errors);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(T value) : base(true, string.Empty)
    {
        Value = value;
    }

    private Result(string error) : base(false, error)
    {
        Value = default;
    }

    private Result(List<string> errors) : base(false, errors)
    {
        Value = default;
    }

    public static new Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(string error) => new(error);
    public static new Result<T> Failure(List<string> errors) => new(errors);
}
