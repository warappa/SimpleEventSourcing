namespace SimpleEventSourcing.Messaging
{
    public static class MessageConstants
    {
        public const string MessageIdKey = "$$messageId$$";
        public const string CorrelationIdKey = "$$correlationId$$";
        public const string CausationIdKey = "$$causationId$$";
        public const string DateTimeKey = "$$dateTime$$";

        public const string CheckpointNumberKey = "$$checkpoint$$";

        public static string GroupKey = "$$group$$";
        public static string CategoryKey = "$$category$$";
    }
}
