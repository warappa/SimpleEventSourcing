using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.State
{
    public abstract class SynchronousEventSourcedState<TState> : ISynchronousEventSourcedState<TState>
        where TState : class, ISynchronousEventSourcedState<TState>, new()
    {
        private static Type[] HandledEventTypes { get; set; }
        private static Type[] HandledMessageTypes { get; set; }
        private static Type[] PayloadTypesStatic { get; set; }
        Type[] IProjector.PayloadTypes => PayloadTypesStatic;

        private static readonly TypeInfo iMessageTypeInfo = typeof(IMessage).GetTypeInfo();
        private static readonly IDictionary<Type, MethodInfo> methodForEventType = new Dictionary<Type, MethodInfo>();
        private static readonly IDictionary<Type, MethodInfo> methodForMessageType = new Dictionary<Type, MethodInfo>();

        static SynchronousEventSourcedState()
        {
            var handledEventTypes = new List<Type>();
            var handledMessageTypes = new List<Type>();
            var parameterTypes = new List<Type>();
            var payloadTypes = new List<Type>();

            var type = typeof(TState).GenericTypeArguments.Length > 0 ? typeof(TState).GenericTypeArguments[0] : typeof(TState);

            var methodInfos = type
                .GetRuntimeMethods()
                .Where(x => x.Name == "Apply")
                .ToList();

            foreach (var methodInfo in methodInfos)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0)
                {
                    continue;
                }

                var parameterType = parameters[0].ParameterType;
                if (parameterType == typeof(object))
                {
                    // skip Apply(object eventOrMessage);
                    continue;
                }

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

        public virtual TState Apply(object eventOrMessage)
        {
            // default noop
            //return this as TState;

            return InvokeAssociatedApply(eventOrMessage);
        }

        protected virtual TState InvokeAssociatedApply(object eventOrMessage)
        {
            var state = this as TState;

            if (eventOrMessage is IMessage message)
            {
                if (methodForMessageType.TryGetValue(message.Body.GetType(), out var mi))
                {
                    object result;

                    result = mi.Invoke(state, new[] { message });
                    state = StateExtensions.ExtractState<TState>(result) ?? state;
                }

                if (methodForEventType.TryGetValue(message.Body.GetType(), out mi))
                {
                    object result;

                    result = mi.Invoke(state, new[] { message.Body });
                    state = StateExtensions.ExtractState<TState>(result) ?? state;
                }
            }
            else
            {
                var @event = eventOrMessage;

                if (methodForEventType.TryGetValue(@event.GetType(), out var mi))
                {
                    object result;

                    result = mi.Invoke(state, new[] { @event });
                    state = StateExtensions.ExtractState<TState>(result) ?? state;
                }
            }

            return state;
        }

        public static TState LoadState(TState state, IEnumerable<object> eventsOrMessages = null)
        {
            state ??= new TState();

            eventsOrMessages ??= Array.Empty<object>();

            foreach (var eventOrMessage in eventsOrMessages)
            {
                state = state.Apply(eventOrMessage).ExtractState<TState>() ?? state;
            }

            return state;
        }

        public static TState LoadState(IStateFactory stateFactory, IEnumerable<object> eventsOrMessages = null)
        {
            var state = stateFactory.CreateState<TState>();

            eventsOrMessages ??= Array.Empty<object>();

            foreach (var eventOrMessage in eventsOrMessages)
            {
                state = state.Apply(eventOrMessage).ExtractState<TState>() ?? state;
            }

            return state;
        }

        object IProjector.UntypedApply(object eventOrMessage)
        {
            return InvokeAssociatedApply(eventOrMessage);
        }

        TState ISynchronousProjectorInternal<TState>.Apply(object @event)
        {
            return InvokeAssociatedApply(@event);
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
