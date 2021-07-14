namespace CosmosDbController
{
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    ///   Used to ensure that creaded through DI cache is initialized properly.
    /// </summary>
    public class ItemsMemoryCache : IItemsMemoryCache
    {
        /// <summary>Gets the cache.</summary>
        /// <value>The cache.</value>
        public MemoryCache Cache { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="ItemsMemoryCache" /> class.</summary>
        public ItemsMemoryCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 100
            });
        }
    }
}
