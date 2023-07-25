using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using AzureMessagingAdventure.Secrets;
using System.Collections.Concurrent;
using System.Text;

namespace AzureMessagingAdventure.EventHub.Receiver
{
    internal class Program
    {
        private static ConcurrentDictionary<string, int> _partitionEventCount;

        static async Task Main(string[] args)
        {
            var eventHubReceiverConfig = SecretReader.ReadUserSecrets<Program, EventHubReceiverConfig>("EventHub");
            var blobContainerClient = new BlobContainerClient(eventHubReceiverConfig.StorageConnectionString,
                                                              eventHubReceiverConfig.StorageContainerName);
            var processorClient = new EventProcessorClient(blobContainerClient,
                                                           "demo",
                                                           eventHubReceiverConfig.EventHubConnectionString,
                                                           eventHubReceiverConfig.EventHubName);

            _partitionEventCount = new ConcurrentDictionary<string, int>();

            processorClient.ProcessEventAsync += ProcessEventAsync;
            processorClient.ProcessErrorAsync += ProcessErrorAsync;

            await processorClient.StartProcessingAsync();

            Console.WriteLine("Processing messages.... hit enter to stop");

            Console.ReadLine();

            await processorClient.StopProcessingAsync();
        }

        private static Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            if (arg.CancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            Console.Error.WriteLine($"Error processing event: ${arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static async Task ProcessEventAsync(ProcessEventArgs arg)
        {
            if (arg.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            var partition = arg.Partition.PartitionId;
            var eventDataContent = Encoding.UTF8.GetString(arg.Data.Body.ToArray());
            Console.WriteLine($"Recieved content from partition {partition}, offset {arg.Data.Offset}: {eventDataContent}");
            
            var eventsSinceLastCheckpoint = _partitionEventCount.AddOrUpdate(
                partition,
                1,
                (_, currentCount) => currentCount + 1);

            if (eventsSinceLastCheckpoint >= 50)
            {
                await arg.UpdateCheckpointAsync();
                _partitionEventCount[partition] = 0;
            }
        }
    }
}