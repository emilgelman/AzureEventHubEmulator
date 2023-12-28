using System.Collections;

namespace AzureEventHubEmulator.Entities;

public class EntityLookup : IEntityLookup
{
    private readonly Dictionary<string, IEntity> _entities;

    public EntityLookup()
    {
        var topicsInput = new Dictionary<string, ITopic>();
        // add a new topic to the dictionary
        topicsInput.Add("/test", new TopicEntity("/test", new[] { "$Default" }));

        var topics = topicsInput.Values
            .Select(topic => new
            {
                topic.Name,
                Entity = (IEntity)topic
            });

        var consumerGroups = topicsInput.Values
            .SelectMany(topic => topic
                .Subscriptions
                .Values
                .Select(subscription => new
                {
                    Name = "/test/ConsumerGroups/$Default/Partitions/0",
                    Entity = (IEntity)subscription
                })
            );


        _entities = topics
            .Concat(consumerGroups)
            // .Concat(queues)
            .ToDictionary(
                item => item.Name,
                item => item.Entity,
                StringComparer.OrdinalIgnoreCase
            );
    }

    public IEntity Find(string name)
        => _entities.TryGetValue(name, out IEntity entity)
            ? entity
            : null;

    public IEnumerator<(string Address, IEntity Entity)> GetEnumerator()
    {
        foreach (KeyValuePair<string, IEntity> item in _entities)
            yield return (item.Key, item.Value);
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}