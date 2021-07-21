namespace CosmosDbController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

/// <summary>
/// Used to communicate to CosmosDB service.
/// </summary>
    public class CosmosDbrepository
    {

        /// <summary>The database identifier</summary>
        private readonly string DatabaseId;

        /// <summary>The container identifier</summary>
        private readonly string ContainerId;

        private readonly CosmosDbInitializerModel cosmosDbInitializerModel;

        private readonly CosmosClient cosmosClient;

        private readonly Lazy<Container> container;

        /// <summary>The items memory cache</summary>
        private readonly IMemoryCache itemsMemoryCache;

        /// <summary>Initializes a new instance of the <see cref="CosmosDbrepository" /> class.</summary>
        /// <param name="itemsMemoryCache">The items memory cache.</param>
        /// <param name="cosmosDbInitializerModel">Cosmos Db initializer model.</param>
        /// <param name="cosmosClient">Cosmos client.</param>
        /// <param name="cosmosDbOptions">CosmosDb configuration options</param>
        public CosmosDbrepository(IMemoryCache itemsMemoryCache, CosmosDbInitializerModel cosmosDbInitializerModel, CosmosClient cosmosClient, IOptions<CosmosDbOptions> cosmosDbOptions)
        {
            var options = cosmosDbOptions.Value;

            this.DatabaseId = options.DatabaseId;
            this.ContainerId = options.ContainerId;

            this.itemsMemoryCache = itemsMemoryCache;
            this.cosmosDbInitializerModel = cosmosDbInitializerModel;
            this.cosmosClient = cosmosClient;

            container = new Lazy<Container>(() => cosmosClient.GetContainer(DatabaseId, ContainerId));
        }

        /// <summary>Creates the item.</summary>
        /// <param name="item">The item to create.</param>
        public async Task<(MovieModel itemMovie, bool created)> CreateItemAsync(MovieModel item)
        {
            await this.EnsureThatCosmosInitializedAsync();

            ResponseMessage response = null;

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, item);
                response = await this.container.Value.UpsertItemStreamAsync(stream, new PartitionKey(item.Year));
            }

            item.ETag = response.Headers["ETag"];

            return (item, response.StatusCode == System.Net.HttpStatusCode.Created);
        }

        /// <summary>Updates the item.</summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="item">The item to update.</param>
        public async Task<(MovieModel itemMovie, bool created)> UpdateItemAsync(string id, MovieModel item)
        {
            await this.EnsureThatCosmosInitializedAsync();

            ResponseMessage response = null;

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, item);
                response = await this.container.Value.UpsertItemStreamAsync(stream, new PartitionKey(item.Year));
            }

            item.ETag = response.Headers["ETag"];

            return (item, response.StatusCode == System.Net.HttpStatusCode.Created);
        }

        /// <summary>Deletes the item.</summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            await this.EnsureThatCosmosInitializedAsync();

            ItemResponse<MovieModel> itemResponse = await this.container.Value.DeleteItemAsync<MovieModel>(id, new PartitionKey(partitionKey));
        }

        /// <summary>Gets the item.</summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        public async Task<MovieModel> GetItemAsync(string id, string partitionKey)
        {
            await this.EnsureThatCosmosInitializedAsync();

            this.itemsMemoryCache.TryGetValue<MovieModel>(MovieModel.ComposeUniqueKey(id, partitionKey), out MovieModel movieItem);

            using (ResponseMessage streamedResponse = await this.container.Value.ReadItemStreamAsync(
               partitionKey: new PartitionKey(partitionKey),
               id: id,
               requestOptions: movieItem == null ? null : new ItemRequestOptions { IfNoneMatchEtag = movieItem.ETag }))
            {
                if (streamedResponse.StatusCode == HttpStatusCode.NotModified)
                {
                    return movieItem;
                }

                if (streamedResponse.IsSuccessStatusCode)
                {
                    movieItem = await streamedResponse.Content.ToObject<MovieModel>();

                    this.itemsMemoryCache.Set(MovieModel.ComposeUniqueKey(id, partitionKey), movieItem, new MemoryCacheEntryOptions { Size = 1 });
                }
                else
                {
                    movieItem = null;
                }
            }

            return movieItem;
        }

        /// <summary>Gets all items.</summary>
        public async Task<IEnumerable<MovieModel>> GetAllItemsAsync()
        {
            await this.EnsureThatCosmosInitializedAsync();

            var sqlQueryText = "SELECT * FROM c";

            var movies = new List<List<MovieModel>>();

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using (FeedIterator queryResultSetStreamIterator = this.container.Value.GetItemQueryStreamIterator(queryDefinition))
            {
                while (queryResultSetStreamIterator.HasMoreResults)
                {
                    using (var currentResultSet = await queryResultSetStreamIterator.ReadNextAsync())
                    {
                        if (currentResultSet.IsSuccessStatusCode)
                        {
                            var pageData = await currentResultSet
                                .Content
                                .ToObject<Dictionary<string, object>>();

                            var movieItems = pageData["Documents"]
                                .ToString()
                                .ToObject<List<MovieModel>>();

                            movies.Add(movieItems);
                        }
                    }
                }
            }

            var allMovies = movies.SelectMany(x => x);
            var memCacheOptions = new MemoryCacheEntryOptions { Size = 1 };
            foreach (var movie in allMovies)
            {
                this.itemsMemoryCache.Set(movie.GetUniqueKey(), movie, memCacheOptions);
            }

            return allMovies;
        }

        /// <summary>Waits for initialization.</summary>
        private async Task EnsureThatCosmosInitializedAsync()
        {
            await this.cosmosDbInitializerModel.InitTask;
        }
    }
}
