using EntityFramework.DbContextScope.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace SimpleEventSourcing.EntityFrameworkCore.Storage
{
    public class DynamicDbContext : DbContext, IDbContext
    {
        public DynamicDbContext(DbConnection connection, bool contextOwnsConnection, DbCompiledModel compiledModel)
            : base(connection, compiledModel, contextOwnsConnection)
        {
            //Database.SetInitializer<DynamicDbContext>(null);
        }

        public static DynamicDbContext Create(DbConnection connection, bool contextOwnsConnection, Type[] typesOfModel)
        {
            var modelBuilder = new ModelBuilder(new Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal..ConventionSet());

            foreach (var type in typesOfModel)
            {
                modelBuilder.Entity(type);
            }

            return new DynamicDbContext(
                connection,
                contextOwnsConnection,
                modelBuilder.Build(connection).Compile()
                );
        }
    }
}
