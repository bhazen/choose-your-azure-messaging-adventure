using Azure.Messaging.EventGrid;
using Azure;
using AzureMessagingAdventure.Secrets;

namespace AzureMessagingAdventure.EventGrid.Sender
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var eventGridSenderConfig = SecretReader.ReadUserSecrets<Program, EventGridSenderConfig>("EventGrid");
            var client = new EventGridPublisherClient(new Uri(eventGridSenderConfig.TopicEndpoint),
                                                      new AzureKeyCredential(eventGridSenderConfig.TopicKey));

            var userSettingUpdatedEvent = new UserSettingUpdatedDemoEvent(Guid.NewGuid(), "Time Zone", "CST", "EST");
            var eventGridEvent = new EventGridEvent("Subject", nameof(UserSettingUpdatedDemoEvent), "1.0.0", userSettingUpdatedEvent);
            
            await client.SendEventAsync(eventGridEvent);

            Console.WriteLine("Event has been sent to EventGrid");
            Console.ReadLine();
        }

        public class UserSettingUpdatedDemoEvent
        {
            public Guid UserId { get; }

            public string SettingName { get; }

            public string OldValue { get; }

            public string NewValue { get; }

            public DateTime Timestamp { get; }

            public UserSettingUpdatedDemoEvent(Guid userId, string settingName, 
                string oldValue, string newValue)
            {
                UserId = userId;
                SettingName = settingName;
                OldValue = oldValue;
                NewValue = newValue;
                Timestamp = DateTime.UtcNow;
            }
        }
    }
}