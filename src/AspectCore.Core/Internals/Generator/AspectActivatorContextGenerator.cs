﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions;
using AspectCore.Core.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal class AspectActivatorContextGenerator : TypeGenerator
    {
        public AspectActivatorContextGenerator() : base(ModuleGenerator.Default.ModuleBuilder)
        {
        }

        public override Type[] Interfaces { get; } = Type.EmptyTypes;

        public override Type ParentType { get; } = typeof(AspectActivatorContext);

        public override TypeAttributes TypeAttributes { get; } = TypeAttributes.Public | TypeAttributes.Sealed;

        public override string TypeName { get; } = "AspectCore.Proxys.DynamicallyAspectActivatorContext";

        public override Type CreateType()
        {
            return ModuleGenerator.Default.DefineTypeInfo(TypeName, key => base.CreateType());
        }

        protected override TypeBuilder ExecuteBuild()
        {
            var typeBuilder = base.ExecuteBuild();

            var properties = ParentType.GetProperties();

            var fields = GetFields(typeBuilder, properties);

            var constructorGenerator = new AspectActivatorContextConstructorGenerator(typeBuilder, fields.ToArray());
            constructorGenerator.Build();

            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(NonAspectAttribute).GetConstructors().First(), EmptyArray<object>.Value));
            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(DynamicallyAttribute).GetConstructors().First(), EmptyArray<object>.Value));

            return typeBuilder;
        }

        private IEnumerable<FieldGenerator> GetFields(TypeBuilder declaringType, PropertyInfo[] properties)
        {
            foreach(var property in properties)
            {
                var propertyGenerator = new AspectActivatorContextPropertyGenerator(declaringType, property);
                propertyGenerator.Build();
                yield return propertyGenerator.Field;
            }
        }
    }
}
