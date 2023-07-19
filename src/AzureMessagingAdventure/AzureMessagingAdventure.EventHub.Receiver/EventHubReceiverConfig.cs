namespace AzureMessagingAdventure.EventHub.Receiver
{
    internal class EventHubReceiverConfig
    {
        public string StorageConnectionString { get; set; }

        public string StorageContainerName { get; set; }

        public string EventHubConnectionString { get; set;}

        public string EventHubName { get; set; }
    }
}
