﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core.Internal
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    return value;
                }

                value = factory(key);
                dictionary.Add(key, value);
                return value;
            }
        }
    }

    public static class AspectValidatorExtensions
    {
        public static bool Validate(this IAspectValidator aspectValidator, Type typeInfo)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            if (typeInfo.IsValueType)
            {
                return false;
            }

            return typeInfo.GetMethods().Any(method => aspectValidator.Validate(method));
        }

        public static bool Validate(this IAspectValidator aspectValidator, PropertyInfo property)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return (property.CanRead && aspectValidator.Validate(property.GetGetMethod())) || (property.CanWrite && aspectValidator.Validate(property.GetSetMethod()));
        }
    }

    public static class ILGeneratorExtensions
    {
        private static readonly Type ILGenType = typeof(Expression).Assembly.GetType("System.Linq.Expressions.Compiler.ILGen");

        private static readonly MethodInfo EmitConvertToTypeMethod = ILGenType.GetMethod("EmitConvertToType", BindingFlags.NonPublic | BindingFlags.Static);

        public static void EmitLoadArg(this ILGenerator ilGenerator, int index)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            switch (index)
            {
                case 0:
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= byte.MaxValue) ilGenerator.Emit(OpCodes.Ldarg_S, (byte)index);
                    else ilGenerator.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        public static void EmitLoadArgA(this ILGenerator ilGenerator, int index)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            if (index <= byte.MaxValue) ilGenerator.Emit(OpCodes.Ldarga_S, (byte)index);
            else ilGenerator.Emit(OpCodes.Ldarga, index);
        }

        public static void EmitConvertToType(this ILGenerator ilGenerator, Type typeFrom, Type typeTo, bool isChecked)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            EmitConvertToTypeMethod.Invoke(null, new object[] { ilGenerator, typeFrom, typeTo, isChecked });
        }

        public static void EmitConvertToObject(this ILGenerator ilGenerator, Type typeFrom)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (typeFrom == null)
            {
                throw new ArgumentNullException(nameof(typeFrom));
            }

            if (typeFrom.IsGenericParameter)
            {
                ilGenerator.Emit(OpCodes.Box, typeFrom);
            }
            else
            {
                ilGenerator.EmitConvertToType(typeFrom, typeof(object), false);
            }
        }

        public static void EmitThis(this ILGenerator ilGenerator)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            ilGenerator.EmitLoadArg(0);
        }

        public static void EmitTypeof(this ILGenerator ilGenerator, Type type)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GetTypeFromHandle);
        }

        public static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            EmitMethodof(ilGenerator, method, method.DeclaringType);
        }

        public static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method, Type declaringType)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            if (declaringType == null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }

            ilGenerator.Emit(OpCodes.Ldtoken, method);
            ilGenerator.Emit(OpCodes.Ldtoken, method.DeclaringType);
            var getMethodFromHandle = ReflectionExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));
            ilGenerator.Emit(OpCodes.Call, getMethodFromHandle);
            ilGenerator.EmitConvertToType(typeof(MethodBase), typeof(MethodInfo), false);
        }

        public static void EmitLoadInt(this ILGenerator ilGenerator, int value)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            switch (value)
            {
                case -1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    ilGenerator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    ilGenerator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    ilGenerator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    ilGenerator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    ilGenerator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (value > -129 && value < 128)
                        ilGenerator.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    else
                        ilGenerator.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }
    }

    public static class ServiceProviderExtensions
    {
        public static IAspectActivator GetAspectActivator(this IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return (IAspectActivator)provider.GetService(typeof(IAspectActivator));
        }
    }

    public static class StringExtensions
    {
        public static unsafe bool Matches(this string input, string pattern)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException(nameof(pattern));

            bool matched = false;

            fixed (char* p_wild = pattern)
            fixed (char* p_str = input)
            {
                char* wild = p_wild, str = p_str, cp = null, mp = null;

                while ((*str) != 0 && (*wild != '*'))
                {
                    if ((*wild != *str) && (*wild != '?')) return matched; wild++; str++;
                }

                while (*str != 0)
                {
                    if (*wild == '*') { if (0 == (*++wild)) return (matched = true); mp = wild; cp = str + 1; }
                    else if ((*wild == *str) || (*wild == '?')) { wild++; str++; } else { wild = mp; str = cp++; }
                }

                while (*wild == '*') wild++; return (*wild) == 0;
            }
        }
    }

    public static class ReflectionExtensions
    {
        public static Type MakeDefType(this Type byRefType)
        {
            if (byRefType == null)
            {
                throw new ArgumentNullException(nameof(byRefType));
            }
            if (!byRefType.IsByRef)
            {
                throw new ArgumentException($"Type {byRefType} is not passed by reference.");
            }

            var assemblyQualifiedName = byRefType.AssemblyQualifiedName;
            var index = assemblyQualifiedName.IndexOf('&');
            assemblyQualifiedName = assemblyQualifiedName.Remove(index, 1);

            return byRefType.Assembly.GetType(assemblyQualifiedName, true);
        }

        public static bool IsProxyType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type.IsDefined(typeof(DynamicallyAttribute), false);
        }

        public static bool CanInherited(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsClass || type.IsSealed)
            {
                return false;
            }

            if (type.IsDefined(typeof(NonAspectAttribute),false) || type.IsProxyType())
            {
                return false;
            }

            if (type.IsNested)
            {
                return type.IsNestedPublic && type.DeclaringType.IsPublic;
            }
            else
            {
                return type.IsPublic;
            }
        }

        public static object FastInvoke(this MethodInfo method, object instance, params object[] parameters)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return new MethodReflector(method).CreateMethodInvoker()(instance, parameters);
        }

        public static TResult FastInvoke<TResult>(this MethodInfo method, object instance, params object[] parameters)
        {
            return (TResult)FastInvoke(method, instance, parameters);
        }

        public static Type[] GetParameterTypes(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return method.GetParameters().Select(parame => parame.ParameterType).ToArray();
        }

        public static bool IsPropertyBinding(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.GetBindingProperty() != null;
        }

        public static PropertyInfo GetBindingProperty(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            foreach (var property in method.DeclaringType.GetProperties())
            {
                if (property.CanRead && property.GetGetMethod() == method)
                {
                    return property;
                }

                if (property.CanWrite && property.GetSetMethod() == method)
                {
                    return property;
                }
            }

            return null;
        }

        public static object FastGetValue(this PropertyInfo property, object instance)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return new PropertyReflector(property).CreatePropertyGetter()(instance);
        }

        public static TReturn FastGetValue<TReturn>(this PropertyInfo property, object instance)
        {
            return (TReturn)FastGetValue(property, instance);
        }

        public static void FastSetValue(this PropertyInfo property, object instance, object value)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            new PropertyReflector(property).CreatePropertySetter()(instance, value);
        }

        public static bool IsNonAspect(this MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }
            return member.IsDefined(typeof(NonAspectAttribute), true);
        }

        internal static MethodInfo GetMethodBySign(this Type type, MethodInfo method)
        {
            return type.GetMethods().FirstOrDefault(m => m.ToString() == method.ToString());
            //if (method.IsGenericMethod)
            //{
            //    foreach (var genericMethod in typeInfo.DeclaredMethods.Where(m => m.IsGenericMethod))
            //    {
            //        if (method.ToString() == genericMethod.ToString())
            //        {
            //            return genericMethod;
            //        }
            //    }
            //}

            //return typeInfo.GetMethod(method.Name, method.GetParameterTypes());
        }

        internal static MethodInfo GetMethod<T>(Expression<T> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            var methodCallExpression = expression.Body as MethodCallExpression;
            if (methodCallExpression == null)
            {
                throw new InvalidCastException("Cannot be converted to MethodCallExpression");
            }
            return methodCallExpression.Method;
        }

        internal static MethodInfo GetMethod<T>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return typeof(T).GetMethod(name);
        }

        internal static MethodInfo ReacquisitionIfDeclaringTypeIsGenericTypeDefinition(this MethodInfo methodInfo, Type closedGenericType)
        {
            if (!methodInfo.DeclaringType.IsGenericTypeDefinition)
            {
                return methodInfo;
            }

            return closedGenericType.GetMethod(methodInfo.Name, methodInfo.GetParameterTypes());
        }

        internal static bool IsCallvirt(this MethodInfo methodInfo)
        {
            var typeInfo = methodInfo.DeclaringType;
            if (typeInfo.IsClass)
            {
                return false;
            }
            return true;
        }

        internal static string GetFullName(this MemberInfo member)
        {
            var declaringType = member.DeclaringType;
            if (declaringType.IsInterface)
            {
                return $"{declaringType.Name}.{member.Name}".Replace('+', '.');
            }
            return member.Name;
        }

        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            return typeof(Task).IsAssignableFrom(methodInfo.ReturnType);
        }

        internal static bool IsAccessibility(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return (property.CanRead && property.GetGetMethod().IsAccessibility()) || (property.CanWrite && property.GetSetMethod().IsAccessibility());
        }

        internal static bool IsAccessibility(this Type declaringType)
        {
            return !(declaringType.IsNotPublic || declaringType.IsValueType || declaringType.IsSealed);
        }

        internal static bool IsAccessibility(this MethodInfo method)
        {
            return !method.IsStatic && !method.IsFinal && method.IsVirtual && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }
    }
}