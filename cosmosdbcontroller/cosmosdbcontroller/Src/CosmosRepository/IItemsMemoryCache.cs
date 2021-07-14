namespace CosmosDbController
{
    using Microsoft.Extensions.Caching.Memory;

    public interface IItemsMemoryCache
    {
        MemoryCache Cache { get; }
    }
}