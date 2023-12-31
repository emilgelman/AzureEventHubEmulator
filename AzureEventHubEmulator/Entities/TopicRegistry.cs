namespace AzureEventHubEmulator.Entities;

public class TopicRegistry : ITopicRegistry
{
    private readonly IReadOnlyDictionary<string, Topic> _topics;

    public TopicRegistry(IEnumerable<string> topicNames)
    {
        _topics = topicNames.ToDictionary(x => x, x => new Topic());
    }

    public Topic? Find(string topicName)
    {
        return _topics.GetValueOrDefault(topicName);
    }
}