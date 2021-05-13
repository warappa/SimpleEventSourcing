using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleEventSourcing.State
{
    public abstract class EventSourcedState<TState> : IEventSourcedState<TState>
        where TState : class, IEventSourcedState<TState>, new()
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

        static EventSourcedState()
        {
            var handledEventTypes = new List<Type>();
            var handledMessageTypes = new List<Type>();
            var parameterTypes = new List<Type>();
            var payloadTypes = new List<Type>();

            var type = typeof(TState).GenericTypeArguments.Length > 0 ? typeof(TState).GenericTypeArguments[0] : typeof(TState);

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

        public virtual TState Apply(object @event)
        {
            // default noop
            return this as TState;
        }

        protected virtual async Task<TState> InvokeAssociatedApplyAsync(object eventOrMessage)
        {
            var state = this as TState;

            if (eventOrMessage is IMessage message)
            {
                if (methodForMessageType.TryGetValue(message.Body.GetType(), out var mi))
                {
                    object result;
                    //if (mi.Name == "Apply")
                    //{
                    //    result = ((dynamic)state).Apply((dynamic)message);
                    //}
                    //else
                    //{
                    //    result = ((dynamic)state).ApplyAsync((dynamic)message);
                    //}

                    result = mi.Invoke(state, new[] { message });
                    state = await StateExtensions.ExtractStateAsync<TState>(result) ?? state;

                    //if (mi.ReturnType != typeof(void))
                    //{
                    //    state = await StateExtensions.ExtractStateAsync<TState>(((dynamic)state).Apply((dynamic)message)) ?? state;
                    //}
                    //else
                    //{
                    //    mi.invoke
                    //    //((dynamic)state).Apply((dynamic)message);
                    //}
                }

                if (methodForEventType.TryGetValue(message.Body.GetType(), out mi))
                {
                    object result;
                    //if (mi.Name == "Apply")
                    //{
                    //    result = ((dynamic)state).Apply((dynamic)message.Body);
                    //}
                    //else
                    //{
                    //    result = ((dynamic)state).ApplyAsync((dynamic)message.Body);
                    //}

                    result = mi.Invoke(state, new[] { message.Body });
                    state = await StateExtensions.ExtractStateAsync<TState>(result) ?? state;
                    //if (mi.ReturnType != typeof(void))
                    //{
                    //    state = await StateExtensions.ExtractStateAsync<TState>(((dynamic)state).Apply((dynamic)message.Body)) ?? state;
                    //}
                    //else
                    //{
                    //    ((dynamic)state).Apply((dynamic)message.Body);
                    //}
                }
            }
            else
            {
                var @event = eventOrMessage;

                if (methodForEventType.TryGetValue(@event.GetType(), out var mi))
                {
                    object result;
                    //if (mi.Name == "Apply")
                    //{
                    //    result = ((dynamic)state).Apply((dynamic)@event);
                    //}
                    //else
                    //{
                    //    result = ((dynamic)state).ApplyAsync((dynamic)@event);
                    //}
                    
                    result = mi.Invoke(state, new[] { @event });
                    state = await StateExtensions.ExtractStateAsync<TState>(result) ?? state;
                    //if (mi.ReturnType != typeof(void))
                    //{
                    //    state = await StateExtensions.ExtractStateAsync<TState>(((dynamic)state).Apply((dynamic)@event)) ?? state;
                    //}
                    //else
                    //{
                    //    ((dynamic)state).Apply((dynamic)@event);
                    //}
                }
            }

            return state;
        }

        public static async Task<TState> LoadStateAsync(TState state, IEnumerable<object> eventsOrMessages = null)
        {
            state = state ?? new TState();

            eventsOrMessages = eventsOrMessages ?? Array.Empty<object>();

            foreach (var eventOrMessage in eventsOrMessages)
            {
                state = await state.Apply(eventOrMessage).ExtractStateAsync<TState>() ?? state;
            }

            return state;
        }

        public static async Task<TState> LoadStateAsync(IStateFactory stateFactory, IEnumerable<object> eventsOrMessages = null)
        {
            var state = stateFactory.CreateState<TState>();

            eventsOrMessages = eventsOrMessages ?? Array.Empty<object>();

            foreach (var eventOrMessage in eventsOrMessages)
            {
                state = await state.Apply(eventOrMessage).ExtractStateAsync<TState>() ?? state;
            }

            return state;
        }

        async Task<object> IState.UntypedApplyAsync(object eventOrMessage)
        {
            return await InvokeAssociatedApplyAsync(eventOrMessage);
        }

        async Task<TState> IStateInternal<TState>.Apply(object @event)
        {
            return await InvokeAssociatedApplyAsync(@event);
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
