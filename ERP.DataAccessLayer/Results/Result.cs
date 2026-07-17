public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result can't have an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public class Result<T> : Result
{
    private readonly T? _data;

    public T Data => IsSuccess
        ? _data!
        : throw new InvalidOperationException("Can't access Data of a failed result.");

    protected internal Result(T? data, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _data = data;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}