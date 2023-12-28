namespace AzureEventHubEmulator.Entities
{
    public interface IEntityLookup : IEnumerable<(string Address, IEntity Entity)>
    {
        IEntity Find(string name);
    }
}
