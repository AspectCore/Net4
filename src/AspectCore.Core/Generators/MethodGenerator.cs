﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions;

namespace AspectCore.Core.Generator
{
    [NonAspect]
    public abstract class MethodGenerator : AbstractGenerator<TypeBuilder, MethodBuilder>
    {
        public abstract string MethodName { get; }

        public abstract MethodAttributes MethodAttributes { get; }

        public abstract CallingConventions CallingConventions { get; }

        public abstract Type ReturnType { get; }

        public abstract Type[] ParameterTypes { get; }

        protected MethodGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override MethodBuilder ExecuteBuild()
        {
            var methodBuilder = DeclaringMember.DefineMethod(MethodName, MethodAttributes, CallingConventions, ReturnType, ParameterTypes);
            var methodBodyGenerator = GetMethodBodyGenerator(methodBuilder);
            GetMethodBodyGenerator(methodBuilder)?.Build();
            return methodBuilder;
        }

        protected abstract MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod);
    }
}
