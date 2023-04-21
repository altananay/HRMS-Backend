namespace Application.Aspects.CustomAspects
{
    public interface IBeforeAspect : IAspect
    {
        object OnBefore();
    }
}