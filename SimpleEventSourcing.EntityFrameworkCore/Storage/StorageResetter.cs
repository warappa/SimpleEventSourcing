using EntityFrameworkCore.DbContextScope;
using Microsoft.EntityFrameworkCore;
using SimpleEventSourcing.Storage;
using System;
using System.Linq;
using System.Transactions;

namespace SimpleEventSourcing.EntityFrameworkCore.Storage
{
    public class StorageResetter<TDbContext> : IStorageResetter
        where TDbContext : DbContext
    {
        private readonly IDbContextScopeFactory dbContextScopeFactory;
        private readonly DbContextOptions options;

        public StorageResetter(IDbContextScopeFactory dbContextScopeFactory, DbContextOptions options)
        {
            this.dbContextScopeFactory = dbContextScopeFactory;
            this.options = options;
        }

        public void Reset(Type[] entityTypes, bool justDrop = false)
        {
            using (var scope = dbContextScopeFactory.Create())
            {
                var originalDbContext = scope.DbContexts.Get<TDbContext>();

                var connection = originalDbContext.Database.GetDbConnection();
                var dbContext = DynamicDbContext.Create(options, connection, entityTypes);
                var emptyDbContext = DynamicDbContext.Create(options, connection, new Type[0]);

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
                                originalDbContext.Database.ExecuteSqlCommand(new RawSqlString(cmd));
                            }
                            catch
                            {
                                // TODO: error handling
                            }
                        }

                        if (!justDrop)
                        {
                            emptyDbContext.Database.EnsureCreated();

                            var creationScript = dbContext.Database.GenerateCreateScript();

                            var creationSteps = creationScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var step in creationSteps)
                            {
                                if (string.IsNullOrWhiteSpace(step))
                                    continue;

                                originalDbContext.Database.ExecuteSqlCommand(new RawSqlString(step));
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

        private string GetTablenameForType(DbContext dbContext, Type type)
        {
            var modelNames = dbContext.Model.FindEntityType(type);
            return modelNames.Relational().TableName;
        }
    }
}
