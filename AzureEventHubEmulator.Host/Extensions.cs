using Amqp.Listener;
using AzureEventHubEmulator.AMQP;
using AzureEventHubEmulator.AMQP.Processors;
using AzureEventHubEmulator.Azure;
using AzureEventHubEmulator.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace AzureEventHubEmulator.Host;

public static class Extensions
{
    public static void AddAzureEventHubEmulator(this IServiceCollection services)
    {
        services.AddSingleton<AzureHandler>();
        services.AddSingleton<AzureSaslProfile>();
        services.AddSingleton<ContainerHostFactory>();
        services.AddSingleton<IContainerHost>(sp =>
        {
            var factory = sp.GetRequiredService<ContainerHostFactory>();
            return factory.Create();
        });
        services.AddSingleton<CbsRequestProcessor>();
        services.AddSingleton<ManagementRequestProcessor>();
        services.AddSingleton<ILinkProcessor, LinkProcessor>();

        services.AddSingleton<ITopicRegistry, TopicRegistry>();

        services.AddSingleton<EventHubEmulatorHost>();
    }
}