using Amqp.Listener;
using AzureEventHubEmulator.AMQP.Processors;
using Microsoft.Extensions.Logging;

namespace AzureEventHubEmulator;

public class EventHubEmulatorHost
{
    private readonly ILogger _logger;
    private readonly IContainerHost _containerHost;
    private readonly ILinkProcessor _linkProcessor;
    private readonly CbsRequestProcessor _cbsRequestProcessor;
    private readonly ManagementRequestProcessor _managementRequestProcessor;


    public EventHubEmulatorHost(ILogger<EventHubEmulatorHost> logger, IContainerHost containerHost, ILinkProcessor linkProcessor, CbsRequestProcessor cbsRequestProcessor,
        ManagementRequestProcessor managementRequestProcessor)

    {
        _logger = logger;
        _containerHost = containerHost;
        _linkProcessor = linkProcessor;
        _cbsRequestProcessor = cbsRequestProcessor;
        _managementRequestProcessor = managementRequestProcessor;
    }

    public async Task StartAsync()
    {
        try
        {
            await Task.Run(() =>
                {
                    _logger.LogInformation("starting");
                    _containerHost.Open();
                    _containerHost.RegisterRequestProcessor("$management", _managementRequestProcessor);
                    _containerHost.RegisterRequestProcessor("$cbs", _cbsRequestProcessor);
                    _containerHost.RegisterLinkProcessor(_linkProcessor);
                })
                .ConfigureAwait(false);
        }
        catch
        {
            _containerHost.Close();
            throw;
        }
    }

    public Task StopAsync()
    {
        _logger.LogInformation("stopping");
        return Task.Run(() => _containerHost.Close());
    }
}