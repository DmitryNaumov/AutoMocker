using System.Linq;
using NUnit.Framework;

namespace NeedfulThings.AutoMocking
{
    [TestFixture]
    public class ConstructorSelectorTests
    {
        [Test]
        public void Should_select_ctor_with_most_dependencies()
        {
            var ctor = new ConstructorSelector().Select(typeof (TestClass));

            CollectionAssert.AreEqual(new [] {typeof(IFoo), typeof(IBar)}, ctor.GetParameters().Select(pi => pi.ParameterType));
        }
    }
}