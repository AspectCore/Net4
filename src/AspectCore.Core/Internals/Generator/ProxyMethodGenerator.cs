﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions;
using AspectCore.Core.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal class ProxyMethodGenerator : GenericMethodGenerator
    {
        const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;

        protected readonly Type _serviceType;
        protected readonly Type _parentType;
        protected readonly MethodInfo _serviceMethod;
        protected readonly FieldBuilder _serviceInstanceFieldBuilder;
        protected readonly FieldBuilder _serviceProviderFieldBuilder;
        protected readonly bool _isImplementExplicitly;

        public ProxyMethodGenerator(TypeBuilder declaringMember, Type serviceType, Type parentType, MethodInfo serviceMethod,
            FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder, bool isImplementExplicitly) : base(declaringMember)
        {
            _serviceType = serviceType;
            _parentType = parentType;
            _serviceMethod = serviceMethod;
            _serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            _serviceProviderFieldBuilder = serviceProviderFieldBuilder;
            _isImplementExplicitly = isImplementExplicitly;
        }

        public override CallingConventions CallingConventions
        {
            get
            {
                return _serviceMethod.CallingConvention;
            }
        }

        public override MethodAttributes MethodAttributes
        {
            get
            {
                if (_serviceType.IsInterface)
                {
                    return ExplicitMethodAttributes;
                }

                var attributes = OverrideMethodAttributes;

                if (_serviceMethod.Attributes.HasFlag(MethodAttributes.Public))
                {
                    attributes = attributes | MethodAttributes.Public;
                }

                if (_serviceMethod.Attributes.HasFlag(MethodAttributes.Family))
                {
                    attributes = attributes | MethodAttributes.Family;
                }

                if (_serviceMethod.Attributes.HasFlag(MethodAttributes.FamORAssem))
                {
                    attributes = attributes | MethodAttributes.FamORAssem;
                }

                return attributes;
            }
        }

        public override string MethodName
        {
            get
            {
                return _isImplementExplicitly ? _serviceMethod.GetFullName() : _serviceMethod.Name;
            }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                return _serviceMethod.GetParameterTypes();
            }
        }

        public override Type ReturnType
        {
            get
            {
                return _serviceMethod.ReturnType;
            }
        }

        public override bool IsGenericMethod
        {
            get
            {
                return _serviceMethod.IsGenericMethod;
            }
        }

        protected override MethodBuilder ExecuteBuild()
        {
            var builder = base.ExecuteBuild();

            if (_serviceType.IsInterface)
            {
                DeclaringMember.DefineMethodOverride(builder, _serviceMethod);
            }

            GeneratingCustomAttribute(builder);
            GeneratingParameters(builder);

            return builder;
        }

        protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
        {
            var parentMethod = _parentType.GetMethodBySign(_serviceMethod);
            return new ProxyMethodBodyGenerator(declaringMethod,
                DeclaringMember,
                _serviceType,
                _serviceMethod,
                parentMethod ?? _serviceMethod,
                _serviceInstanceFieldBuilder,
                _serviceProviderFieldBuilder);
        }

        protected internal override void GeneratingGenericParameter(MethodBuilder declaringMethod)
        {
            var genericArguments = _serviceMethod.GetGenericArguments().ToArray();
            var genericArgumentsBuilders = declaringMethod.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(genericArguments[index].GenericParameterAttributes);
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints())
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint);
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint);
                }
            }
        }

        protected virtual void GeneratingCustomAttribute(MethodBuilder declaringMethod)
        {
            new MethodAttributeGenerator(declaringMethod, typeof(DynamicallyAttribute));
            foreach (var customAttributeData in _serviceMethod.GetCustomAttributesData())
            {
                new MethodAttributeGenerator(declaringMethod, customAttributeData).Build();
            }
        }

        protected virtual void GeneratingParameters(MethodBuilder declaringMethod)
        {
            var parameters = _serviceMethod.GetParameters();
            if (parameters.Length > 0)
            {
                var paramOffset = 1;   // 1
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var parameterBuilder = declaringMethod.DefineParameter(i + paramOffset, parameter.Attributes, parameter.Name);
                    new ParameterAttributeBuilder(parameterBuilder, typeof(DynamicallyAttribute)).Build();
                    foreach (var attribute in parameter.GetCustomAttributesData())
                    {
                        new ParameterAttributeBuilder(parameterBuilder, attribute).Build();
                    }
                }
            }

            var returnParamter = _serviceMethod.ReturnParameter;
            var returnParameterBuilder = declaringMethod.DefineParameter(0, returnParamter.Attributes, returnParamter.Name);
            new ParameterAttributeBuilder(returnParameterBuilder, typeof(DynamicallyAttribute)).Build();
            foreach (var attribute in returnParamter.GetCustomAttributesData())
            {
                new ParameterAttributeBuilder(returnParameterBuilder, attribute).Build();
            }
        }
    }
}