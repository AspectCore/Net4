using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Extensions.Autofac
{
    public class AspectCoreOptions
    {
        public ICollection<IInterceptorFactory> InterceptorFactories { get; } 

        public ICollection<Func<MethodInfo, bool>> NonAspectPredicates { get; }

        public AspectCoreOptions()
        {
            InterceptorFactories = new List<IInterceptorFactory>();

            NonAspectPredicates = new List<Func<MethodInfo, bool>>();

            NonAspectPredicates.Add(m => m.Name.Matches("Equals"));
            NonAspectPredicates.Add(m => m.Name.Matches("GetHashCode"));
            NonAspectPredicates.Add(m => m.Name.Matches("ToString"));
            NonAspectPredicates.Add(m => m.Name.Matches("GetType"));
        }
    }
}