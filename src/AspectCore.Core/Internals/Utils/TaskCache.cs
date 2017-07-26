using System.Threading.Tasks;

namespace AspectCore.Core.Internal
{
    internal static class TaskCache
    {
        internal static readonly Task CompletedTask = FromResult(false);

        private static Task FromResult<T>(T value)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }
    }
}
