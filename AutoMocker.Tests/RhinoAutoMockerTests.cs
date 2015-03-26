using NUnit.Framework;
using Rhino.Mocks;

namespace NeedfulThings.AutoMocking
{
    [TestFixture]
    public class RhinoAutoMockerTests
    {
        [Test]
        public void Should_not_throw_if_dependency_was_setup()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            // request dependency to setup
            var foo = autoMocker.Get<IFoo>();
            foo.Expect(x => x.Method()).Return(1);

            autoMocker.ClassUnderTest.UseFoo();
        }

        [Test]
        public void Should_pass_return_value_from_dependency()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            var foo = autoMocker.Get<IFoo>();
            foo.Expect(x => x.Method()).Return(42);

            int value = autoMocker.ClassUnderTest.UseFoo();

            Assert.AreEqual(42, value);
            foo.VerifyAllExpectations();
        }
    }
}