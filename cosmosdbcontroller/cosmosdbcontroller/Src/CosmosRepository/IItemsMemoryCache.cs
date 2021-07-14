namespace CosmosDbController
{
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    ///   Item memory cache interface for DI
    /// </summary>
    public interface IItemsMemoryCache
    {
        MemoryCache Cache { get; }
    }
}