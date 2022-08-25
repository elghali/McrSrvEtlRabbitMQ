namespace EventBus.Messages.Events
{
    public class ParserFileEvent : IntegrationBaseEvent
    {
        public string OutputFileName { get; set; }
        public string IncomingPath { get; set; }
        public string OutputTableName { get; set; }
    }
}
