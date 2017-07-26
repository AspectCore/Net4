using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AttributeAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 11;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            if (!method.IsPropertyBinding())
            {
                var declaringType = method.DeclaringType;

                if (IsAttributeAspect(declaringType) || IsAttributeAspect(method))
                {
                    return true;
                }
            }

            return next(method);
        }

        private bool IsAttributeAspect(MemberInfo member)
        {
            return member.GetCustomAttributesData().Any(data => typeof(IInterceptor).IsAssignableFrom(data.Constructor.DeclaringType));
        }
    }
}
