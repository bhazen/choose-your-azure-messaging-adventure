using Azure.Messaging.ServiceBus;
using AzureMessagingAdventure.Secrets;

namespace AzureMessagingAdventure.ServiceBus.Receiver
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            var serviceBusReceiverConfig = SecretReader.ReadUserSecrets<Program, ServiceBusReceiverConfig>("ServiceBus");
            await using var client = new ServiceBusClient(serviceBusReceiverConfig.ConnectionString);

            await using var queueProcessor = CreateQueueProcessor(client);
            await queueProcessor.StartProcessingAsync();

            await using var topicProcessor = CreateTopicProcessor(client);
            await topicProcessor.StartProcessingAsync();

            Console.WriteLine("Listening for messages");
            Console.ReadLine();

            await queueProcessor.StopProcessingAsync();
            await topicProcessor.StopProcessingAsync();
        }

        private static ServiceBusProcessor CreateQueueProcessor(ServiceBusClient client)
        {
            var processorOptions = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = true,
                MaxConcurrentCalls = 1,
                PrefetchCount = 1,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };
            var processor = client.CreateProcessor("demo-queue", processorOptions);
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;
            return processor;
        }

        private static ServiceBusProcessor CreateTopicProcessor(ServiceBusClient client)
        {
            var processor = client.CreateProcessor("demo-topic", "demo-subscription");
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;
            return processor;
        }

        private static Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            if (arg.CancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            Console.Error.WriteLine($"An error occurred processing message: {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static Task ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            if (arg.CancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            var message = arg.Message;
            Console.WriteLine($"Received message: {message.SequenceNumber}, {message.Body}");
            return Task.CompletedTask;
        }
    }
}