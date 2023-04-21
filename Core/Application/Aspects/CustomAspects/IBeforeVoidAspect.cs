namespace Application.Aspects.CustomAspects
{
    public interface IBeforeVoidAspect : IAspect
    {
        void OnBefore();
    }
}