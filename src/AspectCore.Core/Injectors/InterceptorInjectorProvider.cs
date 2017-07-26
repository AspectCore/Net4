﻿using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class InterceptorInjectorProvider : IInterceptorInjectorProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPropertyInjectorSelector _propertyInjectorSelector;

        public InterceptorInjectorProvider(
            IServiceProvider serviceProvider,
            IPropertyInjectorSelector propertyInjectorSelector)
        {
            _serviceProvider = serviceProvider;
            _propertyInjectorSelector = propertyInjectorSelector;
        }

        public IInterceptorInjector GetInjector(Type interceptorType)
        {
            var propertyInjectors = _propertyInjectorSelector.SelectPropertyInjector(interceptorType);
            return new InterceptorInjector(_serviceProvider, propertyInjectors);
        }
    }
}
