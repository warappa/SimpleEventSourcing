using SimpleEventSourcing.State;
using System;
using System.Threading.Tasks;

namespace SimpleEventSourcing.Utils
{
    public static class StateExtensions
    {
        public static async Task<TState> ExtractStateAsync<TState>(this object result)
            where TState : class, IState
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
                return (TState)await taskWithObjectResult;
            }
            else if (result is Task<TState> taskWithResult)
            {
                return await taskWithResult;
            }
            else if (result is Task task)
            {
                await task;
                return null;
            }
            else if (result is ValueTask<object> valueTaskWithObjectResult)
            {
                return (TState)await valueTaskWithObjectResult;
            }
            else if (result is ValueTask<TState> valueTaskWithResult)
            {
                return await valueTaskWithResult;
            }
            else if (result is ValueTask valueTask)
            {
                await valueTask;
                return null;
            }

            throw new InvalidOperationException("Method must return Task, Task<TState, ValueTask or ValueTask<TState>");
        }
    }
}
