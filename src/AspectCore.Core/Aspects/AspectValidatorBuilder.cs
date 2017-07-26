﻿using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectValidatorBuilder : IAspectValidatorBuilder
    {
        private readonly IList<Func<AspectValidationDelegate, AspectValidationDelegate>> _collections;

        public AspectValidatorBuilder(IEnumerable<IAspectValidationHandler> aspectValidationHandlers)
        {
            _collections = new List<Func<AspectValidationDelegate, AspectValidationDelegate>>();

            foreach (var handler in aspectValidationHandlers.OrderBy(x => x.Order))
            {
                _collections.Add(next => method => handler.Invoke(method, next));
            }
        }

        public IAspectValidator Build()
        {
            AspectValidationDelegate invoke = method => false;

            var count = _collections.Count;

            for (var i = count - 1; i > -1; i--)
            {
                invoke = _collections[i](invoke);
            }

            return new AspectValidator(invoke);
        }
    }
}
