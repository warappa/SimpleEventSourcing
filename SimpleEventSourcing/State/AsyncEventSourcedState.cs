using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public abstract class AsyncEventSourcedProjector<TProjector> : IEventSourcedState<TProjector>
        where TProjector : class, IEventSourcedState<TProjector>, new()
    {
#pragma warning disable S2743 // Static fields should not be used in generic types
        public static Type[] HandledEventTypes { get; protected set; }
        public static Type[] HandledMessageTypes { get; protected set; }
        public static Type[] PayloadTypesStatic { get; protected set; }
        public Type[] PayloadTypes => PayloadTypesStatic;

        private static readonly TypeInfo iMessageTypeInfo = typeof(IMessage).GetTypeInfo();
        private static readonly IDictionary<Type, MethodInfo> methodForEventType = new Dictionary<Type, MethodInfo>();
        private static readonly IDictionary<Type, MethodInfo> methodForMessageType = new Dictionary<Type, MethodInfo>();
#pragma warning restore S2743 // Static fields should not be used in generic types

        static AsyncEventSourcedProjector()
        {
            var handledEventTypes = new List<Type>();
            var handledMessageTypes = new List<Type>();
            var parameterTypes = new List<Type>();
            var payloadTypes = new List<Type>();

            var type = typeof(TProjector).GenericTypeArguments.Length > 0 ? typeof(TProjector).GenericTypeArguments[0] : typeof(TProjector);

            var methodInfos = type
                .GetRuntimeMethods()
                .Where(x => x.Name == "Apply" ||
                    x.Name == "ApplyAsync")
                .ToList();

            foreach (var methodInfo in methodInfos)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                {
                    continue;
                }

                var parameterType = parameters[0].ParameterType;

                // IMessage<EventType>?
                if (iMessageTypeInfo.IsAssignableFrom(parameterType.GetTypeInfo()))
                {
                    var typedMessageType = parameterType;

                    var eventType = typedMessageType.GetTypeInfo()
                        .GenericTypeArguments[0];

                    parameterTypes.Add(typedMessageType);
                    handledMessageTypes.Add(typedMessageType);
                    payloadTypes.Add(eventType);

                    methodForMessageType.Add(eventType, methodInfo);
                }
                else // EventType?
                {
                    var eventType = parameterType;

                    parameterTypes.Add(eventType);
                    handledEventTypes.Add(eventType);
                    payloadTypes.Add(eventType);

                    methodForEventType.Add(eventType, methodInfo);
                }

                HandledEventTypes = handledEventTypes.ToArray();
                PayloadTypesStatic = payloadTypes.ToArray();
            }
        }

        public static bool HandlesEventOrMessage(object eventOrMessage)
        {
            return HandledMessageTypes
                    .Contains(eventOrMessage.GetType()) ||
                HandledEventTypes
                    .Contains(eventOrMessage.GetType());
        }

        public virtual TProjector Apply(object @event)
        {
            // default noop
            return this as TProjector;
        }

        protected virtual async Task<TProjector> InvokeAssociatedApplyAsync(object eventOrMessage)
        {
            var state = this as TProjector;

            if (eventOrMessage is IMessage message)
            {
                if (methodForMessageType.TryGetValue(message.Body.GetType(), out var mi))
                {
                    object result;

                    result = mi.Invoke(state, new[] { message });
                    state = await StateExtensions.ExtractStateAsync<TProjector>(result).ConfigureAwait(false) ?? state;
                }

                if (methodForEventType.TryGetValue(message.Body.GetType(), out mi))
                {
                    object result;

                    result = mi.Invoke(state, new[] { message.Body });
                    state = await StateExtensions.ExtractStateAsync<TProjector>(result).ConfigureAwait(false) ?? state;
                }
            }
            else
            {
                var @event = eventOrMessage;

                if (methodForEventType.TryGetValue(@event.GetType(), out var mi))
                {
                    object result;

                    result = mi.Invoke(state, new[] { @event });
                    state = await StateExtensions.ExtractStateAsync<TProjector>(result).ConfigureAwait(false) ?? state;
                }
            }

            return state;
        }

        public static async Task<TProjector> LoadStateAsync(TProjector state, IEnumerable<object> eventsOrMessages = null)
        {
            state = state ?? new TProjector();

            eventsOrMessages = eventsOrMessages ?? Array.Empty<object>();

            foreach (var eventOrMessage in eventsOrMessages)
            {
                state = await state.ApplyAsync(eventOrMessage).ExtractStateAsync<TProjector>().ConfigureAwait(false) ?? state;
            }

            return state;
        }

        public static async Task<TProjector> LoadStateAsync(IStateFactory stateFactory, IEnumerable<object> eventsOrMessages = null)
        {
            var state = stateFactory.CreateState<TProjector>();

            eventsOrMessages = eventsOrMessages ?? Array.Empty<object>();

            foreach (var eventOrMessage in eventsOrMessages)
            {
                state = await state.ApplyAsync(eventOrMessage).ExtractStateAsync<TProjector>().ConfigureAwait(false) ?? state;
            }

            return state;
        }

        async Task<object> IAsyncProjector.UntypedApplyAsync(object eventOrMessage)
        {
            return await InvokeAssociatedApplyAsync(eventOrMessage).ConfigureAwait(false);
        }

        async Task<TProjector> IProjectorInternal<TProjector>.ApplyAsync(object @event)
        {
            return await InvokeAssociatedApplyAsync(@event).ConfigureAwait(false);
        }

        object IProjector.UntypedApply(object eventOrMessage)
        {
            return InvokeAssociatedApplyAsync(eventOrMessage); // may return a Task/ValueTask
        }

        public override int GetHashCode()
        {
            return this.PropertyHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.PropertyEqualtity(obj);
        }
    }
}
