using AzureEventHubEmulator.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureEventHubEmulator.Entities;

public class TopicRegistry : ITopicRegistry
{
    private readonly ILogger<TopicRegistry> _logger;
    private readonly IReadOnlyDictionary<string, Topic> _topics;

    public TopicRegistry(ILogger<TopicRegistry> logger, EmulatorOptions emulatorOptions)
    {
        _logger = logger;
        _topics = emulatorOptions.Topics
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToDictionary(x => "/" + x, x => new Topic());

        _logger.LogInformation("Initialized with the following Topics: {Topics}", string.Join(", ", _topics.Keys));
    }

    public Topic? Find(string topicName)
    {
        return _topics.GetValueOrDefault(topicName);
    }
}