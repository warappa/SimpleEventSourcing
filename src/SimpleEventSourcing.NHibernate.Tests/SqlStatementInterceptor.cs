using NHibernate;
using System;
using System.Diagnostics;

namespace SimpleEventSourcing.NHibernate.Tests
{
    [Serializable]
    public class SqlStatementInterceptor : EmptyInterceptor
    {
        public override global::NHibernate.SqlCommand.SqlString OnPrepareStatement(global::NHibernate.SqlCommand.SqlString sql)
        {
            Debug.WriteLine(sql.ToString());
            return sql;
        }
    }
}
