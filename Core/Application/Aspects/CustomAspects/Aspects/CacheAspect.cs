namespace Application.Aspects.CustomAspects.Aspects
{
    public class CacheAspect : AspectBase, IBeforeAspect, IAfterVoidAspect
    {
        public int DurationInMinute { get; set; }

        public object OnBefore()
        {
            string cacheKey = string.Format("{0}_{1}", AspectContext.Instance.MethodName, string.Join("_", AspectContext.Instance.Arguments));

            if (true)
            {
                //todo: Redis cache
                return true;
            }
        }

        public void OnAfter(object value)
        {
            string cacheKey = string.Format("{0}_{1}", AspectContext.Instance.MethodName, string.Join("_", AspectContext.Instance.Arguments));
            //todo: Redis cache
        }
    }
}