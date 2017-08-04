﻿using System;
using NHibernate.Tool.hbm2ddl;
using System.IO;
using SimpleEventSourcing.Storage;

namespace SimpleEventSourcing.NHibernate.Storage
{
    public class StorageResetter : IStorageResetter
    {
        private readonly INHibernateResetConfigurationProvider nHibernateResetConfigurationProvider;

        public StorageResetter(INHibernateResetConfigurationProvider nHibernateResetConfigurationProvider)
        {
            this.nHibernateResetConfigurationProvider = nHibernateResetConfigurationProvider;
        }

        public void Reset(Type[] entityTypes, bool justDrop = false)
        {
            var configuration = nHibernateResetConfigurationProvider.GetConfigurationForTypes(entityTypes);

            using (var sessionFactory = configuration.BuildSessionFactory())
            using (var session = sessionFactory.OpenSession())
            {
                var schemaExport = new SchemaExport(configuration);

                using (var tw = new StringWriter())
                {
                    schemaExport.Execute(true, true, justDrop, session.Connection, tw);
                }
            }
        }
    }
}
