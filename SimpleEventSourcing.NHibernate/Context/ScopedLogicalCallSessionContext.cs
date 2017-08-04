using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace SimpleEventSourcing.NHibernate.Context
{
    [Serializable]
    public class ScopedLogicalCallSessionContext : MapBasedSessionContext
    {
        private const string SessionFactoryMapKey = "75921a87-ef0c-43c7-9b41-c5687df9a0be";

        private static readonly ConcurrentDictionary<string, IDictionary> scopes = new ConcurrentDictionary<string, IDictionary>();

        public ScopedLogicalCallSessionContext(ISessionFactoryImplementor factory)
            : base(factory)
        {
        }

        private static void OpenScope()
        {
            var logicalData = CallContext.LogicalGetData(SessionFactoryMapKey) as string;

            var newScopeValue = Guid.NewGuid().ToString();

            var newLogicalData = string.Join(
                "|",
                (logicalData ?? "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Concat(new[] { newScopeValue }));

            CallContext.LogicalSetData(SessionFactoryMapKey, newLogicalData);

            scopes[newScopeValue] = new Dictionary<object, object>();
        }

        private static void CloseScope()
        {
            var logicalData = CallContext.LogicalGetData(SessionFactoryMapKey) as string;

            var entries = (logicalData ?? "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (entries.Length == 0)
            {
                throw new InvalidOperationException("Cannot close scope - no scope found!");
            }

            var last = entries[entries.Length - 1];

            scopes.TryRemove(last, out IDictionary dummy);

            var newLogicalData = string.Join("|", entries.Take(entries.Length - 1));

            CallContext.LogicalSetData(SessionFactoryMapKey, newLogicalData.Length == 0 ? null : newLogicalData);
        }

        /// <summary>
        /// The key is the session factory and the value is the bound session.
        /// </summary>
        protected override void SetMap(IDictionary value)
        {
            var currentScopeKey = GetCurrentScopeKey();

            if (currentScopeKey == null)
            {
                OpenScope();
                currentScopeKey = GetCurrentScopeKey();
            }

            scopes[currentScopeKey] = value;
        }

        /// <summary>
        /// The key is the session factory and the value is the bound session.
        /// </summary>
        protected override IDictionary GetMap()
        {
            var currentScopeKey = GetCurrentScopeKey();

            if (currentScopeKey == null)
            {
                return null;
            }

            return scopes[currentScopeKey];
        }

        protected override ISession Session
        {
            get => base.Session;
            set
            {
                base.Session = value;

                if (value == null)
                {
                    CloseScope();
                }
            }
        }

        private static string GetCurrentScopeKey()
        {
            var logicalData = CallContext.LogicalGetData(SessionFactoryMapKey) as string;

            return logicalData?.Split('|').LastOrDefault();
        }
    }
}
