using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AzureEventHubEmulator.IntegrationTests;

public class Consumer
{
    private readonly ILogger<Consumer> _logger;
    private readonly EventProcessorClient _processor;
    private readonly IList<string> _events;

    public Consumer(ILogger<Consumer> logger, string storageConnectionString, string blobContainerName, string eventHubConnectionString, string eventHubName)
    {
        _logger = logger;
        _events = new List<string>();
        var storageClient = new BlobContainerClient(storageConnectionString, blobContainerName);

        var processor = new EventProcessorClient(
            storageClient,
            EventHubConsumerClient.DefaultConsumerGroupName,
            eventHubConnectionString,
            eventHubName, new EventProcessorClientOptions()
            {
                PrefetchCount = 300,
                MaximumWaitTime = TimeSpan.FromMinutes(2)
            });

        processor.ProcessEventAsync += ProcessEventHandler;
        processor.ProcessErrorAsync += ProcessErrorHandler;

        _processor = processor;
    }

    public Task StopAsync()
    {
        return _processor.StopProcessingAsync();
    }

    public Task StartAsync()
    {
        return _processor.StartProcessingAsync();
    }

    private Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        var _event = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
        _logger.LogInformation("Received event: {event}", _event);
        _events.Add(_event);
        return Task.CompletedTask;
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
    {
        _logger.LogInformation("Partition '{eventArgs.PartitionId}': an unhandled exception was encountered: {exception}", eventArgs.PartitionId, eventArgs.Exception);
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetEvents()
    {
        return _events;
    }
}