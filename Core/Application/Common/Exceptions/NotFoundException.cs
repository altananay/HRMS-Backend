namespace Application.Common.Exceptions
{
    /// <summary>
    /// The requested resource does not exist. Mapped to HTTP 404 by GlobalExceptionHandler.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string resource, object key)
            : base($"{resource} ({key}) bulunamadı.") { }
    }
}
