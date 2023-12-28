using Microsoft.Extensions.Hosting;

namespace AzureEventHubEmulator.Host;

public class EventHubEmulatorService : IHostedService
{
    private readonly EventHubEmulatorHost _host;

    public EventHubEmulatorService(EventHubEmulatorHost host)
    {
        _host = host;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _host.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _host.StopAsync();
    }
}