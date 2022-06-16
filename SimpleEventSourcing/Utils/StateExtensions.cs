using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Utils
{
    public static class StateExtensions
    {
        public static async Task<TState> ExtractStateAsync<TState>(this object result)
            where TState : class//, IProjector
        {
            if (result == null)
            {
                return null;
            }

            if (result is TState res)
            {
                return res;
            }
            else if (result is Task<object> taskWithObjectResult)
            {
                return (TState)await taskWithObjectResult.ConfigureAwait(false);
            }
            else if (result is Task<TState> taskWithResult)
            {
                return await taskWithResult.ConfigureAwait(false);
            }
            else if (result is Task task)
            {
                await task.ConfigureAwait(false);
                return null;
            }
            else if (result is ValueTask<object> valueTaskWithObjectResult)
            {
                return (TState)await valueTaskWithObjectResult.ConfigureAwait(false);
            }
            else if (result is ValueTask<TState> valueTaskWithResult)
            {
                return await valueTaskWithResult.ConfigureAwait(false);
            }
            else if (result is ValueTask valueTask)
            {
                await valueTask.ConfigureAwait(false);
                return null;
            }

            throw new InvalidOperationException("Method must return Task, Task<TState, ValueTask or ValueTask<TState>");
        }

        public static TState ExtractState<TState>(this object result)
            where TState : class//, ISynchronousProjector
        {
            if (result == null)
            {
                return null;
            }

            if (result is TState res)
            {
                return res;
            }

            throw new InvalidOperationException($"Method must return {typeof(TState).FullName}");
        }
    }
}
