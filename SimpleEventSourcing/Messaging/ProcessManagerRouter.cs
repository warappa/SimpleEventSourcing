using SimpleEventSourcing.Domain;
using SimpleEventSourcing.WriteModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.Messaging
{
    public abstract class ProcessManagerRouter
    {
        protected readonly IProcessManagerRepository processManagerRepository;
        private readonly Func<IMessage, string> processIdExtractor;
        protected IDictionary<Type, List<Type>> eventTypeToProcessManagerTypes = new Dictionary<Type, List<Type>>();

        protected ProcessManagerRouter(IProcessManagerRepository processManagerRepository, Func<IMessage, string> processIdExtractor)
        {
            if (processManagerRepository == null)
            {
                throw new ArgumentNullException(nameof(processManagerRepository));
            }

            if (processIdExtractor == null)
            {
                throw new ArgumentNullException(nameof(processIdExtractor));
            }

            this.processManagerRepository = processManagerRepository;
            this.processIdExtractor = processIdExtractor;
        }

        public void Register<T>()
        {
            var startEventTypes = typeof(T).GetTypeInfo().ImplementedInterfaces
                .Where(interfaceType =>
                    typeof(IProcessManagerStartsWith).GetTypeInfo().IsAssignableFrom(interfaceType.GetTypeInfo()) &&
                    interfaceType.IsConstructedGenericType)
                .ToList();

            if (startEventTypes.Count == 0)
            {
                throw new InvalidOperationException($"Process manager '{typeof(T).FullName}' has no start events!");
            }

            var handleEventTypes = typeof(T).GetTypeInfo().ImplementedInterfaces
                .Where(interfaceType =>
                    typeof(IProcessManagerHandles).GetTypeInfo().IsAssignableFrom(interfaceType.GetTypeInfo()) &&
                    interfaceType.IsConstructedGenericType)
                .ToList();

            foreach (var startEventType in startEventTypes.Concat(handleEventTypes))
            {

                if (eventTypeToProcessManagerTypes.TryGetValue(startEventType, out var processManagerTypes) == false)
                {
                    processManagerTypes = new List<Type>();
                    eventTypeToProcessManagerTypes.Add(startEventType.GetTypeInfo().GenericTypeArguments[0], processManagerTypes);
                }

                processManagerTypes.Add(typeof(T));
            }
        }

        public void Handle(IMessage message)
        {
            var processManagerTypes = GetRegisteredProcessManagerTypesForEventType(message.Body.GetType());

            foreach (var processManagerType in processManagerTypes)
            {
                var processId = processIdExtractor(message);
                var processManager = Load(processManagerType, processId);

                if (processManager == null)
                {
                    var startsWithInterfaces = processManagerType.GetTypeInfo().ImplementedInterfaces
                        .Select(x => x.GetTypeInfo())
                        .Where(x => typeof(IProcessManagerStartsWith).GetTypeInfo().IsAssignableFrom(x))
                        .ToList();

                    var match = startsWithInterfaces
                        .Where(x => x.IsGenericType)
                        .Where(x => x.GenericTypeArguments.Length == 1 &&
                        x.GenericTypeArguments[0] == message.Body.GetType()
                        )
                        .FirstOrDefault();

                    if (match == null)
                    {
                        throw new InvalidOperationException($"'{message.Body.GetType().FullName}' does not match any start events for '{processManagerType.FullName}'!");
                    }

                    processManager = (IProcessManager)Activator.CreateInstance(processManagerType);

                    var startHandle = processManagerType.GetRuntimeMethods()
                        .FirstOrDefault(x =>
                            x.Name == nameof(IProcessManager.Handle) &&
                            x.GetParameters().Count() == 2 &&
                            x.GetParameters()[0].ParameterType == typeof(string) &&
                            x.GetParameters()[1].ParameterType == message.Body.GetType());
                    
                    startHandle.Invoke(processManager, new object[] { processIdExtractor(message), message.Body });
                }
                else
                {
                    ((dynamic)processManager).Handle((dynamic)message.Body);
                }

                processManagerRepository.Save(processManager);
            }
        }

        protected IProcessManager Load(Type processManagerType, string processId)
        {
            return processManagerRepository.Get(processManagerType, processId);
        }

        protected IEnumerable<Type> GetRegisteredProcessManagerTypesForEventType(Type eventType)
        {
            if (!eventTypeToProcessManagerTypes.TryGetValue(eventType, out var processManagerTypes))
            {
                processManagerTypes = new List<Type>();
            }

            return processManagerTypes.ToList();
        }
    }
}
