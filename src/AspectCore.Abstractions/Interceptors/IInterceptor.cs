﻿using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptor
    {
        bool AllowMultiple { get; }

        int Order { get; set; }

        Task Invoke(AspectContext context, AspectDelegate next);
    }
}
