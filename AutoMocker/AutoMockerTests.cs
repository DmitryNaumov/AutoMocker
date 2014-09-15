using System;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Exceptions;

namespace NeedfulThings.AutoMocking
{
    [TestFixture]
    public class AutoMockerTests
    {
        [Test]
        public void Should_not_throw_if_dependency_was_not_setup_and_there_is_no_side_effect()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            autoMocker.ClassUnderTest.NoSideEffect();
        }

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

        [Test]
        public void Should_pass_same_dependency_to_ctor()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            var foo = autoMocker.Get<IFoo>();
            Assert.AreSame(foo, autoMocker.ClassUnderTest.Foo);
        }

        [Test]
        public void Should_return_dependency()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            var foo = autoMocker.Get<IFoo>();
            Assert.NotNull(foo);
        }

        [Test]
        public void Should_throw_if_dependency_requested_after_cut_was_initialized()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();
            autoMocker.ClassUnderTest.Bar.VoidMethod();

            Assert.Throws<InvalidOperationException>(() => autoMocker.Get<IBar>());
        }

        [Test]
        public void Should_throw_if_dependency_was_not_setup_and_method_called_has_out_param()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            Assert.Throws<ExpectationViolationException>(() => autoMocker.ClassUnderTest.OutParam());
        }

        [Test]
        public void Should_throw_if_dependency_was_not_setup_and_method_called_has_ref_param()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            Assert.Throws<ExpectationViolationException>(() => autoMocker.ClassUnderTest.RefParam());
        }

        [Test]
        public void Should_throw_if_dependency_was_not_setup_and_method_called_has_return_value()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            Assert.Throws<ExpectationViolationException>(() => autoMocker.ClassUnderTest.HasReturnValue());
        }

        [Test]
        public void Should_throw_if_dependency_was_not_setup_and_property_getter_called()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            Assert.Throws<ExpectationViolationException>(() => autoMocker.ClassUnderTest.RefParam());
        }

        [Test]
        public void Should_throw_if_wrong_dependency_requested()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            Assert.Throws<InvalidOperationException>(() => autoMocker.Get<IBaz>());
        }

        [Test]
        public void Should_use_ctor_with_most_dependencies()
        {
            var autoMocker = new RhinoAutoMocker<TestClass>();

            Assert.NotNull(autoMocker.ClassUnderTest.Foo);
            Assert.NotNull(autoMocker.ClassUnderTest.Bar);
        }
    }

    public interface IFoo
    {
        int Method();
    }

    public interface IBar
    {
        int Value { get; set; }

        int HasReturnValue();

        void HasArgument(int i);

        void OutParam(out string s);

        void RefParam(ref int i);

        void VoidMethod();
    }

    public interface IBaz
    {
    }

    internal class TestClass
    {
        public TestClass(IFoo foo)
        {
            Foo = foo;
        }

        public TestClass(IBar bar, int i, string s)
        {
            Bar = bar;
        }

        public TestClass(IFoo foo, IBar bar)
        {
            Foo = foo;
            Bar = bar;
        }

        public IFoo Foo { get; private set; }

        public IBar Bar { get; private set; }

        public int UseFoo()
        {
            return Foo.Method();
        }

        public void HasReturnValue()
        {
            Bar.HasReturnValue();
        }

        public void NoSideEffect()
        {
            Bar.Value = 42;
            Bar.VoidMethod();
            Bar.HasArgument(42);
        }

        public void OutParam()
        {
            string s;
            Bar.OutParam(out s);
        }

        public void RefParam()
        {
            int i = 0;
            Bar.RefParam(ref i);
        }

        public void PropertyGetter()
        {
            int value = Bar.Value;
        }
    }
}