using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NHibernate.Cfg;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NHibernate.Mapping.ByCode.Conformist;

namespace SimpleEventSourcing.NHibernate.Storage
{
    public class NHibernateResetConfigurationProvider : INHibernateResetConfigurationProvider
    {
        protected Configuration baseConfiguration;

        private readonly IEnumerable<Assembly> searchAssemblies;

        public NHibernateResetConfigurationProvider(Configuration baseConfiguration, IEnumerable<Assembly> searchAssemblies = null)
        {
            this.baseConfiguration = baseConfiguration;
            this.searchAssemblies = searchAssemblies;
        }

        protected Configuration CloneConfiguration(Configuration configuration)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, configuration);

                ms.Seek(0, SeekOrigin.Begin);

                return (Configuration)bf.Deserialize(ms);
            }
        }

        protected IEnumerable<Type> GetClassMappingTypesForEntityTypes(params Type[] entityTypes)
        {
            return (searchAssemblies ?? AppDomain.CurrentDomain.GetAssemblies())
                .Where(x => x.IsDynamic == false)
                .SelectMany(x => x.GetExportedTypes())
                .Where(x =>
                    x.BaseType?.IsConstructedGenericType == true &&
                    x.BaseType.GetGenericTypeDefinition() == typeof(ClassMapping<>) &&
                    entityTypes.Contains(x.BaseType.GetGenericArguments()[0])
                )
                .ToList();
        }

        public virtual Configuration GetConfigurationForTypes(params Type[] entityTypes)
        {
            var configuration = CloneConfiguration(baseConfiguration);

            var mapper = new ModelMapper();

            var types = GetClassMappingTypesForEntityTypes(entityTypes);

            mapper.AddMappings(types);
            var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            configuration.AddMapping(mapping);

            return configuration;
        }
    }
}
