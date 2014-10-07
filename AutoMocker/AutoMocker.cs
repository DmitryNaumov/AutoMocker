using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeedfulThings.AutoMocking
{
    public class AutoMocker<TClass> where TClass : class
    {
        private readonly Lazy<TClass> _classUnderTest;
        private readonly Dictionary<Type, object> _parameters;
        private readonly IProxyFactory _proxyFactory;

        public AutoMocker(IConstructorSelector constructorSelector, IProxyFactory proxyFactory)
        {
            if (constructorSelector == null)
                throw new ArgumentNullException("constructorSelector");
            if (proxyFactory == null)
                throw new ArgumentNullException("proxyFactory");

            _proxyFactory = proxyFactory;

            ConstructorInfo ctor = constructorSelector.Select(typeof (TClass));

            _parameters = ctor
                .GetParameters()
                .ToDictionary(pi => pi.ParameterType, pi => (object) null);

            _classUnderTest = new Lazy<TClass>(() => CreateInstance(ctor));
        }

        public TClass ClassUnderTest
        {
            get { return _classUnderTest.Value; }
        }

        public T Get<T>() where T : class
        {
            return (T) _proxyFactory.Unwrap(GetMock(typeof (T)));
        }

        public object GetMock(Type type)
        {
            object mock;
            if (!_parameters.TryGetValue(type, out mock))
            {
                throw new InvalidOperationException();
            }

            if (mock == null)
            {
                mock = _proxyFactory.CreateProxy(type);
                _parameters[type] = mock;
            }

            return mock;
        }

        private TClass CreateInstance(ConstructorInfo ctor)
        {
            object[] parameters = ctor.GetParameters()
                .Select(pi => _proxyFactory.Unwrap(_parameters[pi.ParameterType] ?? _proxyFactory.CreateProxy(pi.ParameterType)))
                .ToArray();

            return (TClass) ctor.Invoke(parameters);
        }
    }
}