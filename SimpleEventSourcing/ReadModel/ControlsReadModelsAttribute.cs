using SimpleEventSourcing.State;
using System;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.ReadModel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ControlsReadModelsAttribute : Attribute
    {
        public Type[] ReadModelTypes { get; set; }

        private static Assembly[] knownAssemblies = Array.Empty<Assembly>();
        private static Type[] projectorTypes;

        public ControlsReadModelsAttribute(Type[] viewModelTypes)
        {
            ReadModelTypes = viewModelTypes;
        }

        internal static void ClearKnownAssemblies()
        {
            projectorTypes = null;
            knownAssemblies = Array.Empty<Assembly>();
        }

        public static Type[] GetControlledReadModels(Type projectorType)
        {
            var attr = projectorType.GetTypeInfo().GetCustomAttribute<ControlsReadModelsAttribute>(false);
            if (attr == null)
            {
                return Array.Empty<Type>();
            }

            return attr.ReadModelTypes;
        }

        public static Type GetStateTypeForReadModel(Type readModel, params Assembly[] assemblies)
        {
            if (assemblies.Except(knownAssemblies).Any())
            {
                // if new assemblies are found, reset projectorTypes
                projectorTypes = null;
                knownAssemblies = assemblies.Union(knownAssemblies).ToArray();
            }

            if (knownAssemblies.Length == 0)
            {
                throw new InvalidOperationException("No search assemblies provided and no previous known assemblies found!");
            }

            projectorTypes ??= knownAssemblies
                    .Where(assembly => !assembly.IsDynamic)
                    .SelectMany(assembly => assembly.ExportedTypes)
                    .Where(type => typeof(IProjector).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                    .ToArray()
                ;

            var found = projectorTypes
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
