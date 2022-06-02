using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;
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
            var builder = new DbContextOptionsBuilder<DynamicDbContext>();

            var builderType = builder.GetType();
            var miGeneric = builderType.GetRuntimeMethods().First(x => x.Name == "Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsBuilderInfrastructure.AddOrUpdateExtension");

            foreach (var ext in options.Extensions)
            {
                var mi = miGeneric.MakeGenericMethod(ext.GetType());
                mi.Invoke(builder, new[] { ext });
            }

#if NET6_0_OR_GREATER
            var a = model.GetRelationalModel();
#endif
            builder.UseModel(model);
            var newOptions = builder.Options;
            return newOptions;
        }

        public static DynamicDbContext Create(DbContext dbContext, DbContextOptions options, DbConnection connection, Type[] typesOfModel)
        {
            var conventionSet = ConventionSet.CreateConventionSet(dbContext);

#if NET6_0_OR_GREATER
            var dbContextServices = dbContext.GetService<IDbContextServices>();

            var modelCreationDependencies = dbContextServices.InternalServiceProvider.GetService<ModelCreationDependencies>();

            var conventionSetBuilder = modelCreationDependencies.ConventionSetBuilder;
            var modelConfigurationBuilder = new ModelConfigurationBuilder(conventionSetBuilder.CreateConventionSet());

            //dbContext.ConfigureConventions(modelConfigurationBuilder);

            var modelDependencies = modelCreationDependencies.ModelDependencies;

            var modelBuilder = modelConfigurationBuilder.CreateModelBuilder(modelDependencies);
#else
            var modelBuilder = new ModelBuilder(conventionSet);
#endif

            foreach (var type in typesOfModel)
            {
                modelBuilder.Entity(type);
            }

            foreach (var t in modelBuilder.Model.GetEntityTypes().ToList())
            {
                if (!typesOfModel.Contains(t.ClrType))
                {
                    // workaround return-type change between 3.1.11 and 5.0.2
                    dynamic modelDynamic = modelBuilder.Model;
                    modelDynamic.RemoveEntityType(t.ClrType);
                }
            }

            modelBuilder.BuildIndexesFromAnnotations();

            var model = modelBuilder.FinalizeModel();

#if NET6_0_OR_GREATER
            modelCreationDependencies.ModelRuntimeInitializer.Initialize(model, true);
            modelCreationDependencies.ModelRuntimeInitializer.Initialize(model, false);
#endif
            var newDbContext = new DynamicDbContext(
                options,
                connection,
                model
                );

            return newDbContext;
        }
    }
}
