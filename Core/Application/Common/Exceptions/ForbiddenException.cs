namespace Application.Common.Exceptions
{
    /// <summary>
    /// The caller is authenticated but not allowed to act on this resource — typically an employer
    /// reaching for another employer's advertisement. Mapped to HTTP 403 by GlobalExceptionHandler.
    /// </summary>
    /// <remarks>
    /// Replaces the bare <c>throw new Exception(Messages.Authentication.AuthorizationDenied)</c> in
    /// the deleted SecuredOperation aspect, which surfaced authorization failures as HTTP 500.
    /// </remarks>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Bu işlem için yetkiniz yok.") : base(message) { }
    }
}
