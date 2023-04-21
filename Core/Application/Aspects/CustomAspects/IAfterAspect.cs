namespace Application.Aspects.CustomAspects
{
    public interface IAfterAspect : IAspect
    {
        object OnAfter(object value);
    }
}