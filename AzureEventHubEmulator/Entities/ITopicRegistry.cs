namespace AzureEventHubEmulator.Entities;

public interface ITopicRegistry
{
    Topic? Find(string topicName);
}