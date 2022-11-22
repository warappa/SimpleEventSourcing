using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private static MethodInfo miOnModelCreating;

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
#if NET7_0_OR_GREATER
            var modelConfigurationBuilder = new ModelConfigurationBuilder(conventionSetBuilder.CreateConventionSet(), dbContextServices.InternalServiceProvider);
#else
            var modelConfigurationBuilder = new ModelConfigurationBuilder(conventionSetBuilder.CreateConventionSet());
#endif
            //dbContext.ConfigureConventions(modelConfigurationBuilder);

            var modelDependencies = modelCreationDependencies.ModelDependencies;

            var modelBuilder = modelConfigurationBuilder.CreateModelBuilder(modelDependencies);
#else
            var modelBuilder = new ModelBuilder(conventionSet);
#endif

            using (((Model)modelBuilder.Model).Builder.Metadata.ConventionDispatcher.DelayConventions())
            {
                InvokeOnModelCreating(dbContext, modelBuilder);
            }

            _ = ((Model)modelBuilder.Model).Builder.Metadata.ConventionDispatcher.DelayConventions();
            {
                RemoveForeignKeysToExcludedEntities(typesOfModel, modelBuilder);
                RemoveExcludedEntities(typesOfModel, modelBuilder);
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

        private static void RemoveExcludedEntities(Type[] typesOfModel, ModelBuilder modelBuilder)
        {
            foreach (var t in modelBuilder.Model.GetEntityTypes().ToList())
            {
                if (!typesOfModel.Contains(t.ClrType))
                {
                    // workaround return-type change between 3.1.11 and 5.0.2
                    //modelBuilder.Ignore(t.ClrType);
                    dynamic modelDynamic = modelBuilder.Model;
                    modelDynamic.RemoveEntityType(t.ClrType);
                }
            }
        }

        private static void RemoveForeignKeysToExcludedEntities(Type[] typesOfModel, ModelBuilder modelBuilder)
        {
            foreach (var t in modelBuilder.Model.GetEntityTypes().ToList())
            {
                if (!typesOfModel.Contains(t.ClrType))
                {
                    Fix(t);
                }
            }
        }

        private static void Fix(IMutableEntityType entity)
        {
            var references = entity.GetDeclaredReferencingForeignKeys()
                .ToList();

            foreach (var reference in references)
            {
                var element = reference.DeclaringEntityType;
                foreach (var prop in reference.Properties)
                {
                    element.IsIgnored(prop.Name);
                }

                if (reference.DependentToPrincipal != null)
                {
                    element.IsIgnored(reference.DependentToPrincipal.Name);
                }

                if (reference.PrincipalToDependent != null)
                {
                    entity.IsIgnored(reference.PrincipalToDependent.Name);
                }

                reference.DeclaringEntityType.RemoveForeignKey(reference);
                //reference.DeclaringEntityType.AddForeignKey(currentProps, pk, entity);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        private static void InvokeOnModelCreating(DbContext originalDbContext, ModelBuilder modelBuilder)
        {
            miOnModelCreating ??= typeof(DbContext)
                .GetMethod("OnModelCreating", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(ModelBuilder) }, null);

            miOnModelCreating.Invoke(originalDbContext, new[] { modelBuilder });
        }
    }
}
