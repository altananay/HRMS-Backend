namespace Application.Common.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Bu işlem için yetkiniz yok.") : base(message) { }
    }
}
