using EntityFramework.DbContextScope.Interfaces;
using SimpleEventSourcing.Storage;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleEventSourcing.EntityFramework.Storage
{
    public class StorageResetter<TDbContext> : IStorageResetter
        where TDbContext : DbContext, IDbContext
    {
        private readonly IDbContextScopeFactory dbContextScopeFactory;

        public StorageResetter(IDbContextScopeFactory dbContextScopeFactory)
        {
            this.dbContextScopeFactory = dbContextScopeFactory;
        }

        public async Task ResetAsync(Type[] entityTypes, bool justDrop = false)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                var originalDbContext = scope.DbContexts.Get<TDbContext>();

                var dbContext = DynamicDbContext.Create(originalDbContext, originalDbContext.Database.Connection, false, entityTypes);

                using (var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var tablenames = entityTypes
                            .Select(x => GetTablenameForType(dbContext, x))
                            .ToList();

                        foreach (var tablename in tablenames)
                        {
                            try
                            {
                                dbContext.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $"drop table [{tablename}]");
                            }
                            catch
                            {
                                // TODO: error handling
                            }
                        }

                        if (!justDrop)
                        {
                            var creationScript = ((IObjectContextAdapter)dbContext).ObjectContext.CreateDatabaseScript();

                            var creationSteps = creationScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var step in creationSteps)
                            {
                                if (string.IsNullOrWhiteSpace(step))
                                {
                                    continue;
                                }

                                try
                                {
                                    dbContext.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, step);
                                }
                                catch
                                {
                                    // TODO: error handling
                                }
                            }
                        }

                        transaction.Complete();
                    }
                    catch
                    {
                        // transaction.Rollback();
                    }
                }
            }
        }

        private static string GetTablenameForType(DbContext dbContext, Type type)
        {
            var metadata = ((IObjectContextAdapter)dbContext).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(e => objectItemCollection.GetClrType(e) == type);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var table = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .StoreEntitySet;

            // Return the table name from the storage entity set
            return (string)table.MetadataProperties["Table"].Value ?? table.Name;
        }
    }
}
