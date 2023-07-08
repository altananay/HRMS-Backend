namespace Application.Utilities.Exceptions
{
    public class BusinessException : InvalidOperationException
    {
        public BusinessException(string message) : base(message) { }
    }
}