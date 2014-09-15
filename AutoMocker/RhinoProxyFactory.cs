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
        private static readonly MethodInfo GetIsAnythingMethod = typeof(RhinoProxyFactory).GetMethod("GetIsAnything", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo StubMethod = typeof(RhinoProxyFactory).GetMethod("Stub", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly MockRepository _mockRepository = new MockRepository();

        public object CreateProxy(Type type)
        {
            object proxy = _mockRepository.StrictMock(type);

            List<MethodInfo> safeMethods = type.GetProperties()
                .Select(pi => pi.GetSetMethod())
                .Union(
                    type.GetMethods()
                        .Where(
                            mi =>
                                mi.ReturnType == typeof (void) &&
                                !mi.GetParameters().Any(pi => pi.IsIn || pi.IsOut || pi.ParameterType.IsByRef)))
                .ToList();

            MethodInfo methodInfo = StubMethod.MakeGenericMethod(type);
            foreach (MethodInfo safeMethod in safeMethods)
            {
                methodInfo.Invoke(null, new[] {GetIsAnythingMethod, proxy, safeMethod});
            }

            _mockRepository.Replay(proxy);

            return proxy;
        }

        private static void Stub<T>(MethodInfo mi, T proxy, MethodInfo methodInfo) where T : class
        {
            List<Expression> arguments = methodInfo.GetParameters()
                .Select(pi => (Expression) mi.MakeGenericMethod(pi.ParameterType).Invoke(null, new object[0])).ToList();

            ParameterExpression parameter = Expression.Parameter(typeof (T));
            MethodCallExpression call = Expression.Call(parameter, methodInfo, arguments);
            Expression<Action<T>> lambda = Expression.Lambda<Action<T>>(call, parameter);
            Action<T> action = lambda.Compile();

            proxy.Stub(action);
        }

        private static Expression GetIsAnything<TArg>()
        {
            return Expression.Constant(Arg<TArg>.Is.Anything);
        }
    }
}