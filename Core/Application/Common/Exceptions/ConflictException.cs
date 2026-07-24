namespace Application.Common.Exceptions
{
    /// <summary>
    /// The request conflicts with current state — duplicate email, duplicate job application,
    /// a concurrent update. Mapped to HTTP 409 by GlobalExceptionHandler.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
