// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;

namespace AzureMessagingAdventure.EventGrid.Receiver
{
    public static class EventGridReceiver
    {
        [FunctionName(nameof(ProcessCustomEvent))]
        public static void ProcessCustomEvent([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation($"Received data from cloud event: {eventGridEvent.Data}");
            log.LogInformation($"Event type was {eventGridEvent.EventType}");
            log.LogInformation($"Event subject was {eventGridEvent.Subject}");
            log.LogInformation($"Event version was {eventGridEvent.DataVersion}");
        }
    }
}
