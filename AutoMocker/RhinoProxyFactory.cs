using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rhino.Mocks;

namespace NeedfulThings.AutoMocking
{
    internal sealed class RhinoProxyFactory : IProxyFactory
    {
        private static readonly MethodInfo StubMethod = typeof(RhinoProxyFactory).GetMethod("Stub", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ArgIsAnythingMethod = typeof(RhinoProxyFactory).GetMethod("ArgIsAnything", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly MockRepository _mockRepository = new MockRepository();

        public object CreateProxy(Type type)
        {
            object proxy = _mockRepository.StrictMock(type);

            var stubMethod = StubMethod.MakeGenericMethod(type);
            foreach (var pureMethod in TypeHelper.GetPureMethods(type))
            {
                stubMethod.Invoke(null, new[] {proxy, pureMethod});
            }

            _mockRepository.Replay(proxy);

            return proxy;
        }

        public object Unwrap(object proxy)
        {
            return proxy;
        }

        private static void Stub<T>(T proxy, MethodInfo pureMethod) where T : class
        {
            var arguments = pureMethod.GetParameters()
                .Select(pi => (Expression) ArgIsAnythingMethod.MakeGenericMethod(pi.ParameterType).Invoke(null, new object[0])).ToList();

            var parameter = Expression.Parameter(typeof (T));
            var call = Expression.Call(parameter, pureMethod, arguments);
            var lambda = Expression.Lambda<Action<T>>(call, parameter);
            var action = lambda.Compile();

            proxy.Stub(action);
        }

        private static Expression ArgIsAnything<TArg>()
        {
            return Expression.Constant(Arg<TArg>.Is.Anything, typeof(TArg));
        }
    }
}