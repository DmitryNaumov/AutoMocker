using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeedfulThings.AutoMocking
{
    public static class TypeHelper
    {
        public static IEnumerable<MethodInfo> GetPureMethods(Type type)
        {
            var pureMethods = type.GetProperties().Select(pi => pi.GetSetMethod()).Union(type.GetMethods()
                .Where(mi => mi.ReturnType == typeof(void) && !mi.GetParameters().Any(pi => pi.IsIn || pi.IsOut || pi.ParameterType.IsByRef))).ToList();

            return pureMethods;
        }

        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties().Where(pi => pi.CanWrite);
        }
    }
}