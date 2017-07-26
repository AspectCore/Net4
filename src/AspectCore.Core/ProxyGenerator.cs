﻿using System;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal.Generator;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IAspectValidator _aspectValidator;

        public ProxyGenerator(IAspectValidatorBuilder aspectValidatorBuilder)
        {
            if (aspectValidatorBuilder == null)
            {
                throw new ArgumentNullException(nameof(aspectValidatorBuilder));
            }
            _aspectValidator = aspectValidatorBuilder.Build();
        }

        public Type CreateClassProxyType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (!serviceType.IsClass)
            {
                throw new ArgumentException($"Type '{serviceType}' should be class.", nameof(serviceType));
            }
            return new ClassProxyTypeGenerator(serviceType, implementationType, implementationType.GetInterfaces(), _aspectValidator).CreateType();
        }

        public Type CreateInterfaceProxyType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (!serviceType.IsInterface)
            {
                throw new ArgumentException($"Type '{serviceType}' should be interface.", nameof(serviceType));
            }
            return new InterfaceProxyTypeGenerator(serviceType, implementationType, serviceType.GetInterfaces(), _aspectValidator).CreateType();
        }
    }
}