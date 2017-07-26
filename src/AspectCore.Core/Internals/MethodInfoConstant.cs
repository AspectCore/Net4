using System;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal.Generator;

namespace AspectCore.Core.Internal
{
    internal static class MethodInfoConstant
    {
        internal static readonly MethodInfo GetAspectActivator;

        internal static readonly MethodInfo AspectActivatorInvoke;

        internal static readonly MethodInfo AspectActivatorInvokeAsync;

        internal static readonly MethodInfo ServiceInstanceProviderGetInstance;

        internal static readonly MethodInfo GetTypeFromHandle;

        internal static readonly MethodInfo GetMethodFromHandle;

        internal static readonly ConstructorInfo ArgumentNullExceptionCtor;

        internal static readonly ConstructorInfo AspectActivatorContexCtor;

        internal static readonly ConstructorInfo ObjectCtor;

        static MethodInfoConstant()
        {
            GetAspectActivator = ReflectionExtensions.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

            AspectActivatorInvoke = ReflectionExtensions.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

            AspectActivatorInvokeAsync = ReflectionExtensions.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

            ServiceInstanceProviderGetInstance = ReflectionExtensions.GetMethod<Func<IServiceInstanceProvider, Type, object>>((p, type) => p.GetInstance(type));

            GetTypeFromHandle = ReflectionExtensions.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

            GetMethodFromHandle = ReflectionExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

            ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetConstructor(new Type[] { typeof(string) });

            //var ss = new AspectActivatorContextGenerator().CreateType();

            //var a = ss.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            //AspectActivatorContexCtor =
            //    a.Single();
            ObjectCtor = typeof(object).GetConstructors().Single();
        }
    }
}
