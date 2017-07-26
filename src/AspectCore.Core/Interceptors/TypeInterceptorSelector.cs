using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class TypeInterceptorSelector : IInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo method, Type type)
        {
            if (method.IsPropertyBinding())
            {
                return EmptyArray<IInterceptor>.Value;
            }
            return type.GetCustomAttributes(false).OfType<IInterceptor>();
        }
    }
}
