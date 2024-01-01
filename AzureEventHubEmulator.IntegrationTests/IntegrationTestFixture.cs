using Azure.Storage.Blobs;
using AzureEventHubEmulator.Configuration;
using AzureEventHubEmulator.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.Azurite;

namespace AzureEventHubEmulator.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private IHost? _host;
    private AzuriteContainer? _azuriteContainer;
    private BlobContainerClient _blobClient;
    public Consumer? _consumer;
    public Producer? _producer;

    public async Task InitializeAsync()
    {
        _azuriteContainer = new AzuriteBuilder().WithImage("mcr.microsoft.com/azure-storage/azurite:3.29.0").Build();
        await _azuriteContainer.StartAsync();
        _blobClient = new BlobContainerClient(
            _azuriteContainer.GetConnectionString(), "consumergroup");


        await _blobClient.CreateIfNotExistsAsync();

        _host = CreateHost();
        await _host.StartAsync();
        var loggerFactory = _host.Services.GetRequiredService<ILoggerFactory>();

        const string eventHubConnectionString = "Endpoint=sb://localhost/;SharedAccessKeyName=all;SharedAccessKey=none;EnableAmqpLinkRedirect=false;EntityPath=test";
        const string eventHubName = "test";
        _consumer = new Consumer(loggerFactory.CreateLogger<Consumer>(), _azuriteContainer.GetConnectionString(), "consumergroup",
            eventHubConnectionString, eventHubName);
        _producer = new Producer(loggerFactory.CreateLogger<Producer>(), eventHubConnectionString, eventHubName);
    }

    public async Task DisposeAsync()
    {
        await _azuriteContainer.StopAsync();
        await _host.StopAsync();
    }

    private static IHost CreateHost()
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseEnvironment("IntegrationTest")
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<EmulatorOptions>(sp => new EmulatorOptions()
                {
                    Topics = "test"
                });
                services.AddAzureEventHubEmulator();
            })
            .ConfigureServices(services =>
            {
                services.AddAzureEventHubEmulator();
                services.AddHostedService<EventHubEmulatorService>();
            })
            .Build();
    }
}