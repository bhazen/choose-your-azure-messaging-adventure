using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using AzureMessagingAdventure.Secrets;
using System.Text;

namespace AzureMessagingAdventure.EventHub.Sender
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var eventHubSenderConfig = SecretReader.ReadUserSecrets<Program, EventHubSenderConfig>("EventHub");
            //await SendWithProducerClient(eventHubSenderConfig.ConnectionString, eventHubSenderConfig.EventHubName);
            await SendWithBufferedProducerClient(eventHubSenderConfig.ConnectionString, eventHubSenderConfig.EventHubName);

            Console.WriteLine("Events have been sent");
            Console.ReadLine();
        }

        private static async Task SendWithProducerClient(string connectionString, string eventHubName)
        {
            await using var client = new EventHubProducerClient(connectionString, eventHubName);
            var eventBatch = await client.CreateBatchAsync();

            for (var i = 0; i < 1000; i++)
            {
                var eventData = new EventData(Encoding.UTF8.GetBytes($"Event {i}"));
                eventBatch.TryAdd(eventData);
            }

            await client.SendAsync(eventBatch);

            await client.CloseAsync();
        }

        private static async Task SendWithBufferedProducerClient(string connectionString, string eventHubName)
        {
            await using var client = new EventHubBufferedProducerClient(connectionString, eventHubName);
            client.SendEventBatchSucceededAsync += SendEventBatchSucceededAsync;
            client.SendEventBatchFailedAsync += SendEventBatchFailedAsync;

            for (var i = 0; i < 1000; i++)
            {
                var eventData = new EventData(Encoding.UTF8.GetBytes($"Event {i}"));
                var enqueueOptions = new EnqueueEventOptions { PartitionKey = (i % 2).ToString() };
                await client.EnqueueEventAsync(eventData, enqueueOptions);
            }

            await client.CloseAsync();
        }

        private static Task SendEventBatchFailedAsync(SendEventBatchFailedEventArgs arg)
        {
            Console.Error.WriteLine($"Failed to send data: {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static Task SendEventBatchSucceededAsync(SendEventBatchSucceededEventArgs arg)
        {
            Console.WriteLine($"Successfully sent batch of {arg.EventBatch.Count} events");
            return Task.CompletedTask;
        }
    }
}