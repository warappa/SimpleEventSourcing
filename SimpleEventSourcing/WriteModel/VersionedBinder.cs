using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.WriteModel
{
    public class VersionedBinder : ISerializationBinder
    {
        public const char VersionSeparator = '|';

        private static Dictionary<Type, string> bindTypeToNameDict = new Dictionary<Type, string>();
        private static Dictionary<string, Type> bindNameToTypeDict = new Dictionary<string, Type>();

        private readonly Func<string, Type> bindToTypeFallback;
        private readonly Func<Type, string> bindToNameFallback;
        private readonly Type[] scanAssembliesOfTypes;

        public VersionedBinder(Func<string, Type> bindToTypeFallback = null, Func<Type, string> bindToNameFallback = null, Type[] scanAssembliesOfTypes = null)
        {
            this.bindToTypeFallback = bindToTypeFallback;
            this.bindToNameFallback = bindToNameFallback;
            this.scanAssembliesOfTypes = scanAssembliesOfTypes;
        }

        public string BindToName(Type type)
        {
            if (!bindTypeToNameDict.TryGetValue(type, out string name))
            {
                lock (bindTypeToNameDict)
                {
                    var attr = type.GetTypeInfo().GetCustomAttribute<VersionedAttribute>();

                    if (attr != null)
                    {
                        name = attr.Identifier + VersionSeparator + attr.Version;
                    }
                    else if (bindToNameFallback != null)
                    {
                        name = bindToNameFallback(type);
                    }

                    if (name == null)
                    {
                        name = type.FullName;
                    }

                    bindTypeToNameDict[type] = name;
                }
            }

            return name;
        }

        public Type BindToType(string typename)
        {
            if (!bindNameToTypeDict.TryGetValue(typename, out Type type))
            {
                lock (bindNameToTypeDict)
                {
                    var identifier = typename;
                    var version = 0;

                    var versionParts = typename.Split(VersionSeparator);
                    if (versionParts.Length > 0)
                    {
                        identifier = versionParts[0];
                    }

                    if (versionParts.Length == 2)
                    {
                        version = int.Parse(versionParts[1]);
                    }

                    var typeInfos = GetCurrentAssemblies()
                        .Where(x => !x.IsDynamic)
                        .SelectMany(x => x.ExportedTypes)
                        .Select(x => x.GetTypeInfo())
                        .Where(x => !x.IsAbstract && !x.IsInterface)
                        .Where(x =>
                        {
                            var attr = x.GetCustomAttribute<VersionedAttribute>();
                            if (attr != null)
                            {
                                return attr.Identifier == identifier && attr.Version == version;
                            }
                            else
                            {
                                return x.FullName == typename;
                            }
                        });

                    var typeInfo = typeInfos.FirstOrDefault();

                    type = typeInfo != null ? typeInfo.AsType() :
                        bindToTypeFallback != null ? bindToTypeFallback(typename) : null;

                    bindNameToTypeDict[typename] = type;
                }
            }

            return type;
        }

        private IEnumerable<Assembly> GetCurrentAssemblies()
        {
            Assembly[] assemblies = null;

            if (scanAssembliesOfTypes == null)
            {
                var currentdomain = typeof(string).GetTypeInfo().Assembly.GetType("System.AppDomain")
                    .GetRuntimeProperty("CurrentDomain")
                    .GetMethod
                    .Invoke(null, new object[] { });

                var getassemblies = currentdomain.GetType().GetRuntimeMethod("GetAssemblies", new Type[] { });
                assemblies = getassemblies.Invoke(currentdomain, new object[] { }) as Assembly[];
            }
            else
            {
                assemblies = scanAssembliesOfTypes.Select(x => x.GetTypeInfo().Assembly).ToArray();
            }

            return assemblies.Distinct().ToArray();
        }
    }
}
