using NUnit.Framework;

namespace NeedfulThings.AutoMocking
{
    [TestFixture]
    public class MoqAutoMockerTests
    {
        [Test]
        public void Should_not_throw_if_dependency_was_setup()
        {
            var autoMocker = new MoqAutoMocker<TestClass>();

            // request dependency to setup
            var foo = autoMocker.GetMock<IFoo>();
            foo.Setup(x => x.Method()).Returns(1);

            autoMocker.ClassUnderTest.UseFoo();
        }

        [Test]
        public void Should_pass_return_value_from_dependency()
        {
            var autoMocker = new MoqAutoMocker<TestClass>();

            var foo = autoMocker.GetMock<IFoo>();
            foo.Setup(x => x.Method()).Returns(42);

            int value = autoMocker.ClassUnderTest.UseFoo();

            Assert.AreEqual(42, value);
            foo.VerifyAll();
        }
    }
}