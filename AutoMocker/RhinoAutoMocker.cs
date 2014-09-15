namespace NeedfulThings.AutoMocking
{
    public sealed class RhinoAutoMocker<TClass> : AutoMocker<TClass> where TClass : class
    {
        public RhinoAutoMocker()
            : base(new ConstructorSelector(), new RhinoProxyFactory())
        {
        }
    }
}