using System;
using System.Linq;
using System.Reflection;

namespace NeedfulThings.AutoMocking
{
    internal sealed class ConstructorSelector : IConstructorSelector
    {
        public ConstructorInfo Select(Type type)
        {
            ConstructorInfo[] ctors = type
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .OrderByDescending(ci => GetParameters(ci).Count())
                .ToArray();

            ConstructorInfo ctor = ctors
                .First();

            return ctor;
        }

        private ParameterInfo[] GetParameters(ConstructorInfo ci)
        {
            return
                ci.GetParameters()
                    .Where(
                        pi =>
                            !pi.ParameterType.Assembly.GlobalAssemblyCache &&
                            (pi.ParameterType.IsClass || pi.ParameterType.IsInterface))
                    .ToArray();
        }
    }
}