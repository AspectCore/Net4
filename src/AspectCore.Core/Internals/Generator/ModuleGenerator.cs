using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Core.Internal.Generator
{
    internal sealed class ModuleGenerator
    {
        internal const string ProxyNameSpace = "AspectCore.Proxy";

        private static readonly ModuleGenerator instance = new ModuleGenerator();
        internal static ModuleGenerator Default
        {
            get { return instance; }
        }

        private readonly ModuleBuilder moduleBuilder;
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ConcurrentDictionary<string, Type> createdTypeInfoCache = new ConcurrentDictionary<string, Type>();
        private readonly object cacheLock = new object();

        internal ModuleBuilder ModuleBuilder
        {
            get { return moduleBuilder; }
        }

        private ModuleGenerator()
        {
            var assemblyName = new AssemblyName(ProxyNameSpace);
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("main");
        }

        internal Type DefineTypeInfo(string typeName, Func<string, Type> valueFactory)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            return createdTypeInfoCache.GetOrAdd(typeName, valueFactory);
        }
    }
}
