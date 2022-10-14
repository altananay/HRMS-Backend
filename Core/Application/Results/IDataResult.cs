namespace Application.Results
{
    public interface IDataResult<T> : IResult
    {
        T Data { get; }
    }
}