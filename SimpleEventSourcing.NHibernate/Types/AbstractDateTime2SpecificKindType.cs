namespace SimpleEventSourcing.NHibernate.WriteModel.Types
{/*
    [Serializable]
    public abstract class AbstractDateTime2SpecificKindType : DateTimeType
    {
        protected abstract DateTimeKind DateTimeKind { get; }

        protected AbstractDateTime2SpecificKindType()
            : base(SqlTypeFactory.DateTime2)
        {
        }

        #region from DateTime2 type (no public constructor!!!)

        public override object Next(object current, global::NHibernate.Engine.ISessionImplementor session)
        {
            return Seed(session);
        }

        public override object Seed(global::NHibernate.Engine.ISessionImplementor session)
        {
            return DateTime.UtcNow;
        }

        #endregion

        public override object FromStringValue(string xml)
        {
            return DateTime.SpecifyKind(DateTime.Parse(xml), DateTimeKind);
        }

        public override void Set(IDbCommand st, object value, int index)
        {
            var dateValue = (DateTime)value;
            ((IDataParameter)st.Parameters[index]).Value = CreateDateTime(dateValue);
        }

        public override object Get(IDataReader rs, int index)
        {
            try
            {
                var dbValue = Convert.ToDateTime(rs[index]);
                return CreateDateTime(dbValue);
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[index]), ex);
            }
        }

        public override int GetHashCode(object x, EntityMode entityMode)
        {
            int hashCode = base.GetHashCode(x, entityMode);
            unchecked
            {
                hashCode = 31 * hashCode + ((DateTime)x).Kind.GetHashCode();
            }
            return hashCode;
        }

        public override bool IsEqual(object x, object y)
        {
            if (x == y)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return base.IsEqual(x, y) && ((DateTime)x).Kind == ((DateTime)y).Kind;
        }

        protected virtual DateTime CreateDateTime(DateTime dateValue)
        {
            return new DateTime(dateValue.Ticks, DateTimeKind);
        }
    }*/
}
