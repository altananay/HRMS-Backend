namespace Application.Results
{
    public interface IResult
    {
        bool IsSuccess { get; }
        string? Message { get; }
    }

    public interface IDataResult<out T> : IResult
    {
        T Data { get; }
    }

    public class Result : IResult
    {
        public Result(bool isSuccess, string? message = null)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public bool IsSuccess { get; }
        public string? Message { get; }
    }

    public class DataResult<T> : Result, IDataResult<T>
    {
        public DataResult(T data, bool isSuccess, string? message = null) : base(isSuccess, message)
            => Data = data;

        public T Data { get; }
    }

    public class SuccessResult : Result
    {
        public SuccessResult(string? message = null) : base(true, message) { }
    }

    public class ErrorResult : Result
    {
        public ErrorResult(string? message = null) : base(false, message) { }
    }

    public class SuccessDataResult<T> : DataResult<T>
    {
        public SuccessDataResult(T data, string? message = null) : base(data, true, message) { }
    }

    public class ErrorDataResult<T> : DataResult<T>
    {
        public ErrorDataResult(T data, string? message = null) : base(data, false, message) { }
    }
}
