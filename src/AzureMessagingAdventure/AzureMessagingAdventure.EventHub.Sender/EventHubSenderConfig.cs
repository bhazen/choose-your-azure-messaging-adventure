namespace AzureMessagingAdventure.EventHub.Sender
{
    internal class EventHubSenderConfig
    {
        public string ConnectionString { get; set; }
        
        public string EventHubName { get; set; }
    }
}
