using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace SimpleEventSourcing.NHibernate.WriteModel
{
    public class RawSnapshotMapping : ClassMapping<RawSnapshot>
    {
        public RawSnapshotMapping()
        {
            Table("Snapshots");

            Property(x => x.StreamName, config =>
            {
                config.Column(x =>
                {
                    x.SqlType("nvarchar(255)");
                    x.Length(255);
                });

                config.Length(100);

                config.NotNullable(true);
                config.Index("LoadSnapshots");
                config.UniqueKey("LoadSnapshots");

                /* Unmerged change from project 'SimpleEventSourcing.NHibernate (netcoreapp2.1)'
                Before:
                            });

                            Property(x => x.StreamRevision, config =>
                After:
                            });

                            Property(x => x.StreamRevision, config =>
                */

                /* Unmerged change from project 'SimpleEventSourcing.NHibernate (net5.0)'
                Before:
                            });

                            Property(x => x.StreamRevision, config =>
                After:
                            });

                            Property(x => x.StreamRevision, config =>
                */

                /* Unmerged change from project 'SimpleEventSourcing.NHibernate (net6.0)'
                Before:
                            });

                            Property(x => x.StreamRevision, config =>
                After:
                            });

                            Property(x => x.StreamRevision, config =>
                */

                /* Unmerged change from project 'SimpleEventSourcing.NHibernate (netcoreapp3.1)'
                Before:
                            });

                            Property(x => x.StreamRevision, config =>
                After:
                            });

                            Property(x => x.StreamRevision, config =>
                */
            });

            Property(x => x.StreamRevision, config =>
            {
                config.NotNullable(true);
                config.Index("LoadSnapshots");
                config.UniqueKey("LoadSnapshots");
            });
            Property(x => x.StateIdentifier, config =>
            {
                config.Column(x =>
                {
                    x.SqlType("nvarchar(255)");
                    x.Length(255);
                });

                config.Length(255);

                config.NotNullable(true);
                config.Index("LoadSnapshots");
                config.UniqueKey("LoadSnapshots");
            });
            Property(x => x.StateSerialized, config =>
            {
                config.Type(NHibernateUtil.StringClob);
                config.NotNullable(true);
            });

            Property(x => x.CreatedAt, config =>
            {
                config.Type(new UtcDateTimeType());
            });
            Id(x => x.Id, config =>
            {
                //config.Generator(global::NHibernate.Mapping.ByCode.Generators.HighLow);
                //config.Generator(global::NHibernate.Mapping.ByCode.Generators.Identity);
                //config
                //    .Generator(global::NHibernate.Mapping.ByCode.Generators.SequenceHiLo, x =>
                //    {
                //        x.Params(new
                //        {
                //            //next_hi = 1000,
                //            max_lo = 1000,
                //            table = "commits_hilo"
                //        });
                //    });
                config.Generator(global::NHibernate.Mapping.ByCode.Generators.SequenceIdentity);
            });
        }
    }
}
