using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using System.Collections.Generic;

namespace SimpleEventSourcing.NHibernate.WriteModel
{
    public class RawStreamEntryMapping : ClassMapping<RawStreamEntry>
    {
        public RawStreamEntryMapping()
        {
            Table("Commits");

            Property(x => x.StreamName, config =>
            {
                config.Column(x =>
                {
                    x.SqlType("nvarchar(255)");
                    x.Length(255);
                });

                config.Length(100);

                config.NotNullable(true);
                config.Index("LoadStreamMessages");
            });
            Property(x => x.CommitId, config =>
            {
                config.Column(x =>
                {
                    x.SqlType("nvarchar(255)");
                    x.Length(255);
                });
            });
            Property(x => x.MessageId, config =>
            {
                config.Column(x =>
                {
                    x.SqlType("nvarchar(255)");
                    x.Length(255);
                });
            });
            Property(x => x.StreamRevision, config =>
            {
                config.NotNullable(true);
            });
            Property(x => x.PayloadType, config =>
            {
                config.Column(x =>
                {
                    x.SqlType("nvarchar(255)");
                    x.Length(255);
                });

                config.Length(255);

                config.NotNullable(true);
                config.Index("LoadStreamMessages");
            });
            Property(x => x.Payload, config =>
            {
                config.Type(NHibernateUtil.StringClob);
                config.NotNullable(true);
            });
            Property(x => x.Group, config =>
            {
                config.Index("LoadStreamMessages");
                config.Column(x =>
                {
                    x.Name("`Group`");
                    x.SqlType("nvarchar(255)");
                    x.Length(255);
                });
            });
            Property(x => x.Category, config =>
            {
                //config.Index("LoadStreamMessages");
                config.Column(x =>
                {
                    x.SqlType("nvarchar(MAX)");
                    x.Length(8000);
                });
            });
            Property(x => x.Headers, config =>
            {
                config.Type(NHibernateUtil.StringClob);
                config.NotNullable(true);
                config.Column(x =>
                {
                    x.SqlType("nvarchar(MAX)");
                    x.Length(8000);
                });
            });
            Property(x => x.DateTime, config =>
            {
                config.Type(new UtcDateTimeType());
            });
            Property(x => x.CheckpointNumber, config =>
            {
                config.Index("LoadStreamMessages");
            });
            Id(x => x.CheckpointNumber, config =>
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
