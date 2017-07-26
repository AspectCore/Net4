﻿using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    [NonAspect]
    public class NonAspectValidationHandler : IAspectValidationHandler
    {
        private readonly IAspectConfigureProvider _aspectConfigureProvider;

        public NonAspectValidationHandler(IAspectConfigureProvider aspectConfigureProvider)
        {
            _aspectConfigureProvider = aspectConfigureProvider;
        }

        public int Order { get; } = 3;

        public virtual bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            var declaringType = method.DeclaringType;
            if (method.IsNonAspect() || declaringType.IsNonAspect())
            {
                return false;
            }
            if (_aspectConfigureProvider.AspectConfigure.NonAspectPredicates.Any(x => x(method)))
            {
                return false;
            }
            return next(method);
        }
    }
}
