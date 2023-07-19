using Azure.Messaging.ServiceBus;
using AzureMessagingAdventure.Secrets;

namespace AzureMessagingAdventure.ServiceBus.Sender
{
    internal class Program
    {
        async static Task Main(string[] args)
        {
            var serviceBusSenderConfig = SecretReader.ReadUserSecrets<Program, ServiceBusSenderConfig>("ServiceBus");
            await using var client = new ServiceBusClient(serviceBusSenderConfig.ConnectionString);

            await using var queueSender = client.CreateSender("demo-queue");
            await SendMessage(queueSender, "Test message for a queue");

            await using var topicSender = client.CreateSender("demo-topic");
            await SendMessage(topicSender, "This is a message for a topic");

            Console.WriteLine("Demo messageas have been sent");
            Console.ReadLine();

            await queueSender.CloseAsync();
            await topicSender.CloseAsync();
        }

        private static async Task SendMessage(ServiceBusSender sender, string messageText)
        {
            var message = new ServiceBusMessage(messageText);
            await sender.SendMessageAsync(message);
        }

        private static async Task SendMessageBatchAsync(ServiceBusSender sender, int numberOfMessagesToSend)
        {
            var messageBatch = await sender.CreateMessageBatchAsync();
            for (var i = 0; i < numberOfMessagesToSend; i++)
            {
                messageBatch.TryAddMessage(new ServiceBusMessage($"Mesage {i}"));
            }

            await sender.SendMessagesAsync(messageBatch);
        }
    }
}