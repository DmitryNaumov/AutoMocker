﻿using System;

namespace NeedfulThings.AutoMocking
{
    public interface IProxyFactory
    {
        object CreateProxy(Type type);
    }
}