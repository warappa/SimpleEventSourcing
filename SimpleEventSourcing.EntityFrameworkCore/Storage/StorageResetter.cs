using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SimpleEventSourcing.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SimpleEventSourcing.EntityFrameworkCore.Storage
{
    public class StorageResetter<TDbContext> : IStorageResetter, 
        IReadModelStorageResetter, IWriteModelStorageResetter
        where TDbContext : DbContext
    {
        private readonly IDbContextScopeFactory dbContextScopeFactory;
        private readonly DbContextOptions options;

        public StorageResetter(IDbContextScopeFactory dbContextScopeFactory, DbContextOptions<TDbContext> options)
        {
            this.dbContextScopeFactory = dbContextScopeFactory;
            this.options = options;
        }

        public async Task ResetAsync(Type[] entityTypes, bool justDrop = false)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                var originalDbContext = scope.DbContexts.Get<TDbContext>();

                var connection = originalDbContext.Database.GetDbConnection();
                var dbContext = DynamicDbContext.Create(originalDbContext, options, connection, entityTypes);
                var emptyDbContext = DynamicDbContext.Create(originalDbContext, options, connection, Array.Empty<Type>());

                if (!justDrop)
                {
                    emptyDbContext.Database.EnsureCreated();
                }
                 
                using (var transaction = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {

                    try
                    {
                        var tablenames = entityTypes
                            .Select(x => GetTablenameForType(dbContext, x))
                            .ToList();

                        var tablenames2 = entityTypes
                            .Select(x => GetTablenameForType(originalDbContext, x))
                            .ToList();

                        if (!tablenames.SequenceEqual(tablenames2))
                        {
                            throw new Exception("not equal");
                        }

                        foreach (var tablename in tablenames)
                        {
                            try
                            {
                                var cmd = $"drop table [{tablename}]";
                                originalDbContext.Database.ExecuteSqlRaw(cmd);
                            }
                            catch
                            {
                                // TODO: error handling
                            }
                        }

                        if (!justDrop)
                        {
                            var creationScript = dbContext.GetService<IRelationalDatabaseCreator>().GenerateCreateScript();

                            var creationSteps = creationScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var step in creationSteps)
                            {
                                if (string.IsNullOrWhiteSpace(step))
                                {
                                    continue;
                                }

                                try
                                {
                                    originalDbContext.Database.ExecuteSqlRaw(step);
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
            var modelNames = dbContext.Model.FindEntityType(type);
            return modelNames.GetTableName();
        }
    }
}
