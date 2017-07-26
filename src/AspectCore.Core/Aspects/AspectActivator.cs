using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectActivator : IAspectActivator
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;


        public AspectActivator(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory)
        {
            _aspectContextFactory = aspectContextFactory;
            _aspectBuilderFactory = aspectBuilderFactory;
        }

        public TReturn Invoke<TReturn>(AspectActivatorContext activatorContext)
        {
            var invokeAsync = InvokeAsync<TReturn>(activatorContext);

            if (invokeAsync.IsFaulted)
            {
                throw invokeAsync.Exception?.InnerException;
            }

            if (invokeAsync.IsCompleted)
            {
                return invokeAsync.Result;
            }

            return invokeAsync.GetAwaiter().GetResult();
        }

        public async Task<TReturn> InvokeAsync<TReturn>(AspectActivatorContext activatorContext)
        {
            using (var context = _aspectContextFactory.CreateContext<TReturn>(activatorContext))
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                await aspectBuilder.Build()(() => context.Target.Invoke(context.Parameters))(context);
                return await Unwrap<TReturn>(context.ReturnParameter.Value);
            }
        }

        private async Task<TReturn> Unwrap<TReturn>(object value)
        {
            if (value is Task<TReturn>)
            {
                return await (Task<TReturn>)value;
            }
            else if (value is Task)
            {
                await (Task)value;
                return default(TReturn);
            }
            else
            {
                return (TReturn)value;
            }
        }
    }
}
