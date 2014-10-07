using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;

namespace NeedfulThings.AutoMocking
{
    internal sealed class MoqProxyFactory : IProxyFactory
    {
        private static readonly MethodInfo CreateProxyInternalMethod = typeof(MoqProxyFactory).GetMethod("CreateProxyInternal", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo SetupMethod = typeof(MoqProxyFactory).GetMethod("Setup", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ItIsAnyMethod = typeof(MoqProxyFactory).GetMethod("ItIsAny", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Strict);

        public object CreateProxy(Type type)
        {
            return CreateProxyInternalMethod.MakeGenericMethod(type).Invoke(this, null);
        }

        public object Unwrap(object proxy)
        {
            return ((Mock) proxy).Object;
        }

        private Mock<T> CreateProxyInternal<T>() where T : class
        {
            var proxy = _mockRepository.Create<T>();

            var setupMethod = SetupMethod.MakeGenericMethod(typeof(T));
            foreach (var pureMethod in TypeHelper.GetPureMethods(typeof(T)))
            {
                setupMethod.Invoke(null, new object[] { proxy, pureMethod });
            }

            foreach (var pureMethod in TypeHelper.GetProperties(typeof(T)))
            {
                setupMethod.Invoke(null, new object[] { proxy, pureMethod });
            }

            return proxy;
        }

        private static void Setup<T>(Mock<T> proxy, MemberInfo memberInfo) where T : class
        {
            var pureMethod = memberInfo as MethodInfo;
            if (pureMethod != null)
            {
                if (pureMethod.IsSpecialName)
                    return;

                var arguments = pureMethod.GetParameters()
                    .Select(pi => (Expression) ItIsAnyMethod.MakeGenericMethod(pi.ParameterType).Invoke(null, new object[0]))
                    .ToList();

                var parameter = Expression.Parameter(typeof (T));
                var call = Expression.Call(parameter, pureMethod, arguments);
                var lambda = Expression.Lambda<Action<T>>(call, parameter);

                proxy.Setup(lambda);
                return;
            }

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                var parameter = Expression.Parameter(typeof(T));
                
                var memberAccess = Expression.Property(parameter, propertyInfo);
                var assingment = Expression.Assign(memberAccess, (Expression) ItIsAnyMethod.MakeGenericMethod(propertyInfo.PropertyType).Invoke(null, new object[0]));
                var lambda = Expression.Lambda<Action<T>>(assingment, parameter);

                proxy.SetupSet(lambda.Compile());

                return;
            }

            throw new ArgumentException("memberInfo");
        }

        private static Expression ItIsAny<TArg>()
        {
            Expression<Func<TArg>> lambda = () => It.IsAny<TArg>();
            return lambda.Body;
        }

    }
}