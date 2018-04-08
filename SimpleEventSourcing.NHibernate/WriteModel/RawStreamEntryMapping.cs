using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using SimpleEventSourcing.NHibernate.WriteModel.Types;

namespace SimpleEventSourcing.NHibernate.WriteModel
{
    public class RawStreamEntryMapping : ClassMapping<RawStreamEntry>
    {
        public RawStreamEntryMapping()
        {
            Property(x => x.StreamName, config =>
            {
                config.Column(x =>
                {
                    x.SqlType("varchar(100)");
                    x.Length(100);
                });

                config.Length(100);

                config.NotNullable(true);
                config.Index("LoadStreamMessages");
            });
            Property(x => x.CommitId);
            Property(x => x.MessageId);
            Property(x => x.StreamRevision);
            Property(x => x.PayloadType, config =>
            {
                config.Column(x =>
                {
                    x.SqlType("varchar(200)");
                    x.Length(200);
                });

                config.Length(200);

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
                config.Column("`Group`");
            });
            Property(x => x.Category);
            Property(x => x.Headers, config =>
            {
                config.Type(NHibernateUtil.StringClob);
                config.NotNullable(true);
            });
            Property(x => x.DateTime, config =>
            {
                config.Type(new UtcDateTimeType());
            });
            Id(x => x.CheckpointNumber, config =>
            {
                config.Generator(global::NHibernate.Mapping.ByCode.Generators.HighLow);
            });
        }
    }
}
