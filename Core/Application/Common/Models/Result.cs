namespace Application.Results
{
    /// <summary>
    /// Outcome envelope for a successful operation.
    /// </summary>
    /// <remarks>
    /// Kept from the original design (CLAUDE.md §3.4 requires it) but its role is narrowed: it is
    /// now only a <b>success</b> envelope. Failures travel as typed exceptions and are turned into
    /// ProblemDetails by GlobalExceptionHandler.
    ///
    /// That split fixes a real problem. Previously every read path returned
    /// <c>SuccessDataResult</c> unconditionally — business rules threw instead of returning an
    /// error result — so the <c>else return BadRequest(...)</c> branch in all 14 controllers was
    /// unreachable code that merely looked like error handling.
    ///
    /// The namespace stays <c>Application.Results</c> so the ~60 feature files keep compiling; only
    /// the folder moved, from Utilities/Results to Common/Models.
    /// </remarks>
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

    /// <remarks>
    /// The old version had a <c>SuccessDataResult(string message)</c> overload passing
    /// <c>default</c> as data, which for <c>T = string</c> silently bound the data argument to the
    /// message parameter. Those overloads are gone — data is always explicit.
    /// </remarks>
    public class SuccessDataResult<T> : DataResult<T>
    {
        public SuccessDataResult(T data, string? message = null) : base(data, true, message) { }
    }

    public class ErrorDataResult<T> : DataResult<T>
    {
        public ErrorDataResult(T data, string? message = null) : base(data, false, message) { }
    }
}
