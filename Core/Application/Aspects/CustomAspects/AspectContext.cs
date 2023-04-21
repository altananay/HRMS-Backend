namespace Application.Aspects.CustomAspects
{
    public class AspectContext
    {
        private readonly static Lazy<AspectContext> _instance = new Lazy<AspectContext>(() => new AspectContext());
        private AspectContext() { }

        public static AspectContext Instance { get {  return _instance.Value; } }

        public string MethodName { get; set; }
        public object[] Arguments { get; set; }
    }
}