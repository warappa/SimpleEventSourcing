using NHibernate;
using System;
using System.Diagnostics;

[Serializable]
public class SqlStatementInterceptor : EmptyInterceptor
{
    public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
    {
        Debug.WriteLine(sql.ToString());
        return sql;
    }
}
