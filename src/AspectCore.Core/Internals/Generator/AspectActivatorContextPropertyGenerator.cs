﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Core.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal class AspectActivatorContextPropertyGenerator : PropertyGenerator
    {
        private readonly PropertyInfo _property;

        public AspectActivatorContextPropertyGenerator(TypeBuilder declaringMember,PropertyInfo property) : base(declaringMember)
        {
            _property = property;
        }

        public override CallingConventions CallingConventions { get; } = CallingConventions.HasThis;

        public override bool CanRead { get; } = true;

        public override bool CanWrite { get; } = false;

        public override MethodInfo GetMethod
        {
            get
            {
                return _property.GetGetMethod();
            }
        }

        public override PropertyAttributes PropertyAttributes
        {
            get
            {
                return _property.Attributes;
            }
        }

        public override string PropertyName
        {
            get
            {
                return _property.Name;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return _property.PropertyType;
            }
        }

        public override MethodInfo SetMethod
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FieldGenerator Field
        {
            get; set;
        }

        protected override MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType)
        {
            throw new NotImplementedException();
        }

        protected override MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType)
        {
            return new ContextPropertyMethodGenerator(declaringType, GetMethod, Field);
        }

        protected override PropertyBuilder ExecuteBuild()
        {
            Field = new ProxyFieldGnerator($"_{PropertyName}", PropertyType, DeclaringMember);
            Field.Build();

            return base.ExecuteBuild();
        }

        private class ContextPropertyMethodGenerator : MethodGenerator
        {
            private readonly MethodInfo method;

            private readonly FieldGenerator field;

            public ContextPropertyMethodGenerator(TypeBuilder declaringMember, MethodInfo method, FieldGenerator field)
                : base(declaringMember)
            {
                this.method = method;
                this.field = field;
            }

            public override CallingConventions CallingConventions
            {
                get
                {
                    return method.CallingConvention;
                }
            }

            public override MethodAttributes MethodAttributes
            {
                get
                {
                    return MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
                }
            }

            public override string MethodName
            {
                get
                {
                    return method.Name;
                }
            }

            public override Type[] ParameterTypes
            {
                get
                {
                    return Type.EmptyTypes;
                }
            }

            public override Type ReturnType
            {
                get
                {
                    return method.ReturnParameter.ParameterType;
                }
            }

            protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
            {
                return new ContextPropertyMethodGeneratorBody(declaringMethod, field);
            }
        }

        private class ContextPropertyMethodGeneratorBody : MethodBodyGenerator
        {
            private readonly FieldGenerator _field;
            public ContextPropertyMethodGeneratorBody(MethodBuilder declaringMember, FieldGenerator field) : base(declaringMember)
            {
                _field = field;
            }

            protected override void GeneratingMethodBody(ILGenerator ilGenerator)
            {
                ilGenerator.EmitThis();
                ilGenerator.Emit(OpCodes.Ldfld, _field.Build());
                ilGenerator.Emit(OpCodes.Ret);
            }
        }
    }
}
