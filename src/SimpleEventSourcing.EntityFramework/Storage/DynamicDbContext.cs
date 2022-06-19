using EntityFramework.DbContextScope.Interfaces;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.EntityFramework.Storage
{
    public class DynamicDbContext : DbContext, IDbContext
    {
        private static MethodInfo miOnModelCreating;

        public DynamicDbContext(DbConnection connection, bool contextOwnsConnection, DbCompiledModel compiledModel)
            : base(connection, compiledModel, contextOwnsConnection)
        {
            Database.SetInitializer<DynamicDbContext>(null);
        }

        public static DynamicDbContext Create(DbContext originalDbContext, DbConnection connection, bool contextOwnsConnection, Type[] typesOfModel)
        {
            var modelBuilder = new DbModelBuilder();

            InvokeOnModelCreating(originalDbContext, modelBuilder);

            foreach (var type in typesOfModel)
            {
                modelBuilder.RegisterEntityType(type);
            }

            var types = modelBuilder.Build(connection).ConceptualModel.EntityTypes;
            foreach (var t in types)
            {
                var clrType = (Type)t.MetadataProperties["http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType"].Value;
                if (!typesOfModel.Contains(clrType))
                {
                    modelBuilder.Ignore(new[] { clrType });
                }
            }

            return new DynamicDbContext(
                connection,
                contextOwnsConnection,
                modelBuilder.Build(connection).Compile()
            );
        }

        private static void InvokeOnModelCreating(DbContext originalDbContext, DbModelBuilder modelBuilder)
        {
            miOnModelCreating ??= typeof(DbContext)
                .GetMethod("OnModelCreating", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(DbModelBuilder) }, null);

            miOnModelCreating.Invoke(originalDbContext, new[] { modelBuilder });
        }
    }
}
