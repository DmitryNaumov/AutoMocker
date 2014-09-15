using System;
using System.Reflection;

namespace NeedfulThings.AutoMocking
{
    public interface IConstructorSelector
    {
        ConstructorInfo Select(Type type);
    }
}