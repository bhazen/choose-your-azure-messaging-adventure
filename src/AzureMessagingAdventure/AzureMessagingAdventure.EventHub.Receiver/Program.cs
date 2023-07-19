using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using AzureMessagingAdventure.Secrets;
using System.Text;

namespace AzureMessagingAdventure.EventHub.Receiver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var eventHubReceiverConfig = SecretReader.ReadUserSecrets<Program, EventHubReceiverConfig>("EventHub");
            var blobContainerClient = new BlobContainerClient(eventHubReceiverConfig.StorageConnectionString,
                                                              eventHubReceiverConfig.StorageContainerName);
            var processorClient = new EventProcessorClient(blobContainerClient,
                                                           "demo",
                                                           eventHubReceiverConfig.EventHubConnectionString,
                                                           eventHubReceiverConfig.EventHubName);

            processorClient.ProcessEventAsync += ProcessEventAsync;
            processorClient.ProcessErrorAsync += ProcessErrorAsync;

            await processorClient.StartProcessingAsync();

            Console.WriteLine("Processing messages.... hit enter to stop");

            Console.ReadLine();

            await processorClient.StopProcessingAsync();
        }

        private static Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            Console.Error.WriteLine($"Error processing event: ${arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static Task ProcessEventAsync(ProcessEventArgs arg)
        {
            var eventDataContent = Encoding.UTF8.GetString(arg.Data.Body.ToArray());
            Console.WriteLine($"Recieved contennt from partition {arg.Partition.PartitionId}, offset {arg.Data.Offset}: {eventDataContent}");
            return Task.CompletedTask;
        }
    }
}