using Moq;

namespace NeedfulThings.AutoMocking
{
    public sealed class MoqAutoMocker<TClass> : AutoMocker<TClass> where TClass : class
    {
        public MoqAutoMocker() : base(new ConstructorSelector(), new MoqProxyFactory())
        {
        }

        public Mock<T> GetMock<T>() where T : class
        {
            return (Mock<T>) base.GetMock(typeof (T));
        }
    }
}