using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SimpleEventSourcing.EntityFrameworkCore.Internal;
using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.EntityFrameworkCore.Storage
{
    public class DynamicDbContext : DbContext
    {
        private DynamicDbContext(DbContextOptions options)
            : base(options)
        {

        }
        private DynamicDbContext(DbContextOptions options, DbConnection connection, IModel compiledModel)
            : base(BuildOptions(options, connection, compiledModel))
        {
        }

        private static DbContextOptions BuildOptions(DbContextOptions options, DbConnection connection, IModel model)
        {
            var builder = new DbContextOptionsBuilder<DynamicDbContext>()
                .UseModel(model);

            var newExtension = options.Extensions.OfType<RelationalOptionsExtension>().First().WithConnection(connection).WithConnectionString(connection.ConnectionString);
            var builderType = ((IDbContextOptionsBuilderInfrastructure)builder).GetType();
            var actualExtensioType = newExtension.GetType();
            var a = builderType.GetRuntimeMethods();
            var miGeneric = builderType.GetRuntimeMethods().First(x => x.Name == "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsBuilderInfrastructure.AddOrUpdateExtension");
            var mi = miGeneric.MakeGenericMethod(actualExtensioType);
            //((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(newExtension);
            mi.Invoke(builder, new[] { newExtension });

            var newOptions = builder.Options;
            var fieldInfos = newExtension.GetType().BaseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            fieldInfos.First(x => x.Name == "_connectionString").SetValue(newExtension, null);
            return newOptions;
        }

        public static DynamicDbContext Create(DbContext dbContext, DbContextOptions options, DbConnection connection, Type[] typesOfModel)
        {
            var conventionSet = ConventionSet.CreateConventionSet(dbContext);

            var modelBuilder = new ModelBuilder(conventionSet);
            
            foreach (var type in typesOfModel)
            {
                modelBuilder.Entity(type);
            }

            foreach(var t in modelBuilder.Model.GetEntityTypes().ToList())
            {
                if (!typesOfModel.Contains(t.ClrType))
                {
                    modelBuilder.Model.RemoveEntityType(t);
                }
            }

            modelBuilder.BuildIndexesFromAnnotations();

            return new DynamicDbContext(
                options,
                connection,
                modelBuilder.Model
                );
        }
    }
}
