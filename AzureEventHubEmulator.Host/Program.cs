using AzureEventHubEmulator.Configuration;
using AzureEventHubEmulator.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<EmulatorOptions>(hostContext.Configuration.GetSection("Emulator"));
        services.AddSingleton<EmulatorOptions>(sp => { return hostContext.Configuration.GetSection("Emulator").Get<EmulatorOptions>(); });
        services.AddAzureEventHubEmulator();
        services.AddHostedService<EventHubEmulatorService>();
    })
    .Build()
    .RunAsync();