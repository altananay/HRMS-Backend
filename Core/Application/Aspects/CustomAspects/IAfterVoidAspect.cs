namespace Application.Aspects.CustomAspects
{
    public interface IAfterVoidAspect : IAspect
    {
        void OnAfter(object value);
    }
}