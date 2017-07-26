﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions;

namespace AspectCore.Core.Internal.Generator
{
    internal class ClassProxyTypeGenerator : ProxyTypeGenerator
    {
        public ClassProxyTypeGenerator(Type serviceType, Type parentType, Type[] interfaces, IAspectValidator aspectValidator) : base(serviceType, aspectValidator)
        {
            if (!parentType.CanInherited())
            {
                throw new InvalidOperationException($"Validate '{parentType}' failed because the type does not satisfy the condition to be inherited.");
            }
            ParentType = parentType;
            Interfaces = interfaces?.Distinct().Where(i => aspectValidator.Validate(i))?.ToArray() ?? Type.EmptyTypes;
        }

        public override string TypeName => $"{ParentType.Namespace}.{ParentType.Name}Proxy";

        public override Type[] Interfaces { get; }

        public override Type ParentType { get; }

        protected override void GeneratingConstructor(TypeBuilder declaringType)
        {
            var constructors = ParentType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(c => !c.IsStatic && (c.IsPublic || c.IsFamily || c.IsFamilyAndAssembly || c.IsFamilyOrAssembly)).ToArray();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"A suitable constructor for type {ServiceType.FullName} could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
            }
            foreach (var constructor in constructors)
            {
                new ProxyConstructorGenerator(declaringType, ServiceType, constructor, serviceInstanceFieldBuilder, serviceProviderFieldBuilder).Build();
            }
        }

        protected override void GeneratingMethod(TypeBuilder declaringType)
        {
            foreach (var method in ServiceType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.IsPropertyBinding())
                {
                    continue;
                }
                if (AspectValidator.Validate(method))
                {
                    new ProxyMethodGenerator(declaringType, ServiceType, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, false).Build();
                    continue;
                }
                if (method.IsAccessibility())
                {
                    new NonProxyMethodGenerator(declaringType, ParentType, method, serviceInstanceFieldBuilder, false).Build();
                }
            }

            foreach (var @interface in Interfaces)
            {
                foreach (var method in @interface.GetMethods())
                {
                    if (method.IsPropertyBinding())
                    {
                        continue;
                    }
                    if (!AspectValidator.Validate(method))
                    {
                        new NonProxyMethodGenerator(declaringType, ParentType, method, serviceInstanceFieldBuilder, true).Build();
                        continue;
                    }
                    new ProxyMethodGenerator(declaringType, @interface, ParentType, method, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, true).Build();
                }
            }
        }

        protected override void GeneratingProperty(TypeBuilder declaringType)
        {
            foreach (var property in ServiceType.GetProperties())
            {
                if (AspectValidator.Validate(property))
                {
                    new ProxyPropertyGenerator(declaringType, property, ServiceType, ParentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, false).Build();
                }
                else if (property.IsAccessibility())
                {
                    new NonProxyPropertyGenerator(declaringType, property, ServiceType, ParentType, serviceInstanceFieldBuilder, false).Build();
                }
            }

            foreach (var @interface in Interfaces)
            {
                foreach (var property in @interface.GetProperties())
                {
                    if (AspectValidator.Validate(property))
                    {
                        new ProxyPropertyGenerator(declaringType, property, @interface, ParentType, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, true).Build();
                    }
                    else
                    {
                        new NonProxyPropertyGenerator(declaringType, property, @interface, ParentType, serviceInstanceFieldBuilder, true).Build();
                    }
                }
            }
        }
    }
}