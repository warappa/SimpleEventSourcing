using NHibernate.Cfg;
using System;

namespace SimpleEventSourcing.NHibernate.Storage
{
    public interface INHibernateResetConfigurationProvider
    {
        Configuration GetConfigurationForTypes(params Type[] entityTypes);
    }
}
