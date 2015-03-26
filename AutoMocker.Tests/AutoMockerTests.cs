using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Rhino.Mocks.Exceptions;

namespace NeedfulThings.AutoMocking
{
    [TestFixture]
    public class AutoMockerTests
    {
        private static readonly IResolveConstraint ExpectedExceptions =
            new OrConstraint(new ExactTypeConstraint(typeof (ExpectationViolationException)), new ExactTypeConstraint(typeof(MockException)));

        [TestCaseSource("AutoMockerFactory")]
        public void Should_not_throw_if_dependency_was_not_setup_and_there_is_no_side_effect(AutoMocker<TestClass> autoMocker)
        {
            autoMocker.ClassUnderTest.NoSideEffect();
        }

        [TestCaseSource("AutoMockerFactory")]
        public void Should_pass_same_dependency_to_ctor(AutoMocker<TestClass> autoMocker)
        {
            var foo = autoMocker.Get<IFoo>();
            Assert.AreSame(foo, autoMocker.ClassUnderTest.Foo);
        }

        [TestCaseSource("AutoMockerFactory")]
        public void Should_return_dependency(AutoMocker<TestClass> autoMocker)
        {
            var foo = autoMocker.Get<IFoo>();
            Assert.NotNull(foo);
        }

        [TestCaseSource("AutoMockerFactory")]
        public void Should_return_dependency_even_after_cut_was_initialized(AutoMocker<TestClass> autoMocker)
        {
            autoMocker.ClassUnderTest.Bar.VoidMethod();

            var bar = autoMocker.Get<IBar>();
        }

        [TestCaseSource("AutoMockerFactory")]
        public void Should_throw_if_dependency_was_not_setup_and_method_called_has_out_param(AutoMocker<TestClass> autoMocker)
        {
            Assert.Throws(ExpectedExceptions, () => autoMocker.ClassUnderTest.OutParam());
        }

        [TestCaseSource("AutoMockerFactory")]
        public void Should_throw_if_dependency_was_not_setup_and_method_called_has_ref_param(AutoMocker<TestClass> autoMocker)
        {
            Assert.Throws(ExpectedExceptions, () => autoMocker.ClassUnderTest.RefParam());
        }

        [TestCaseSource("AutoMockerFactory")]
        public void Should_throw_if_dependency_was_not_setup_and_method_called_has_return_value(AutoMocker<TestClass> autoMocker)
        {
            Assert.Throws(ExpectedExceptions, () => autoMocker.ClassUnderTest.HasReturnValue());
        }

        [TestCaseSource("AutoMockerFactory")]
        public void Should_throw_if_dependency_was_not_setup_and_property_getter_called(AutoMocker<TestClass> autoMocker)
        {
            Assert.Throws(ExpectedExceptions, () => autoMocker.ClassUnderTest.PropertyGetter());
        }

        [TestCaseSource("AutoMockerFactory")]
        public void Should_throw_if_wrong_dependency_requested(AutoMocker<TestClass> autoMocker)
        {
            Assert.Throws<InvalidOperationException>(() => autoMocker.Get<IBaz>());
        }

        private IEnumerable<AutoMocker<TestClass>> AutoMockerFactory()
        {
            yield return new RhinoAutoMocker<TestClass>();
            yield return new MoqAutoMocker<TestClass>();
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
        
        void HasMultipleArgument(int i, string s, double d);

        void OutParam(out string s);

        void RefParam(ref int i);

        void VoidMethod();
    }

    public interface IBaz
    {
    }

    public class TestClass
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
            Bar.HasMultipleArgument(42, "bar", 1.0);
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