using SimpleEventSourcing.State;
using System;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.ReadModel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ControlsReadModelsAttribute : Attribute
    {
        public Type[] ViewModelTypes { get; set; }

        private static Assembly[] knownAssemblies = new Assembly[0];
        private static Type[] stateTypes;

        public ControlsReadModelsAttribute(Type[] viewModelTypes)
        {
            ViewModelTypes = viewModelTypes;
        }

        internal static void ClearKnownAssemblies()
        {
            stateTypes = null;
            knownAssemblies = new Assembly[0];
        }

        public static Type[] GetControlledReadModels(Type stateModel)
        {
            var attr = stateModel.GetTypeInfo().GetCustomAttribute<ControlsReadModelsAttribute>(false);
            if (attr == null)
            {
                return new Type[0];
            }

            return attr.ViewModelTypes;
        }

        public static Type GetStateTypeForReadModel(Type readModel, params Assembly[] assemblies)
        {
            if (assemblies.Except(knownAssemblies).Any())
            {
                // if new assemblies are found, reset projectorTypes
                stateTypes = null;
                knownAssemblies = assemblies.Union(knownAssemblies).ToArray();
            }

            if (knownAssemblies.Length == 0)
            {
                throw new InvalidOperationException("No search assemblies provided and no previous known assemblies found!");
            }

            stateTypes = stateTypes ?? (
                stateTypes = knownAssemblies
                    .Where(assembly => !assembly.IsDynamic)
                    .SelectMany(assembly => assembly.ExportedTypes)
                    .Where(type => typeof(IState).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                    .ToArray()
                );

            var found = stateTypes
                .Where(projectorType =>
                    GetControlledReadModels(projectorType)
                        .Any(readModelType => readModelType == readModel))
                .ToList();

            if (found.Count > 1)
            {
                throw new InvalidOperationException($"Found multiple states claiming to control read model '{readModel.FullName}': {string.Join(", ", found.Select(x => x.FullName))}");
            }

            return found.SingleOrDefault();
        }
    }
}
