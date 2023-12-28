using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Logging;

namespace AzureEventHubEmulator.IntegrationTests;

public class Producer
{
    private readonly ILogger<Producer> _logger;
    private readonly EventHubProducerClient _client;

    public Producer(ILogger<Producer> logger, string connectionString, string eventHubName)
    {
        _logger = logger;
        _client = new EventHubProducerClient(connectionString, eventHubName);
    }

    public async Task SendAsync(string body)
    {
        var events = new List<EventData> { new(body) };

        try
        {
            await _client.SendAsync(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending message to EventHub");
        }
    }
}