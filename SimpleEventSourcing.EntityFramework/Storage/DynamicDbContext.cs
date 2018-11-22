using EntityFramework.DbContextScope.Interfaces;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace SimpleEventSourcing.EntityFramework.Storage
{
    public class DynamicDbContext : DbContext, IDbContext
    {
        public DynamicDbContext(DbConnection connection, bool contextOwnsConnection, DbCompiledModel compiledModel)
            : base(connection, compiledModel, contextOwnsConnection)
        {
            Database.SetInitializer<DynamicDbContext>(null);
        }

        public static DynamicDbContext Create(DbConnection connection, bool contextOwnsConnection, Type[] typesOfModel)
        {
            var modelBuilder = new DbModelBuilder();

            foreach (var type in typesOfModel)
            {
                modelBuilder.RegisterEntityType(type);
            }
            
            return new DynamicDbContext(
                connection,
                contextOwnsConnection,
                modelBuilder.Build(connection).Compile()
                );
        }
    }
}
