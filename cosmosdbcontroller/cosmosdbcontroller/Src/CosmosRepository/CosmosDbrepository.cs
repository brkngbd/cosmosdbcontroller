namespace CosmosDbController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Storage.Core.Util;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Used to communicate to CosmosDB service.
    /// </summary>
    public class CosmosDbrepository : BackgroundService, ICosmosDbrepository
    {
        /// <summary>The database identifier</summary>
        private const string DatabaseId = "dbnew1";

        /// <summary>The container identifier</summary>
        private const string ContainerId = "items";

        /// <summary>The items memory cache</summary>
        private readonly IMemoryCache itemsMemoryCache;

        /// <summary>The Cosmos client instance</summary>
        private CosmosClient cosmosClient;

        /// <summary>The database we will create.</summary>
        private Database database;

        /// <summary>The container we will create.</summary>
        private Container container;

        /// <summary>The event used to ensure that CosmosDb is initialized.</summary>
        private ManualResetEvent eventInitialized = new ManualResetEvent(false);

        /// <summary>The CosmosDb initialization error message.</summary>
        private string initializationError = null;

        /// <summary>Initializes a new instance of the <see cref="CosmosDbrepository" /> class.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="itemsMemoryCache">The items memory cache.</param>
        public CosmosDbrepository(IConfiguration configuration, IMemoryCache itemsMemoryCache)
        {
            var endpointUri = configuration["EndpointUri"];
            var primaryKey = configuration["PrimaryKey"];

            this.cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });

            this.itemsMemoryCache = itemsMemoryCache;
        }

        /// <summary>Creates the item.</summary>
        /// <param name="item">The item to create.</param>
        public async Task<(Movie itemMovie, bool created)> CreateItemAsync(Movie item)
        {
            this.EnsureThatCosmosInitialized();

            ResponseMessage response = null;

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, item);
                response = await this.container.UpsertItemStreamAsync(stream, new PartitionKey(item.Year));
            }

            item.ETag = response.Headers["ETag"];

            return (item, response.StatusCode == System.Net.HttpStatusCode.Created);
        }

        /// <summary>Updates the item.</summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="item">The item to update.</param>
        public async Task<(Movie itemMovie, bool created)> UpdateItemAsync(string id, Movie item)
        {
            this.EnsureThatCosmosInitialized();

            ResponseMessage response = null;

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, item);
                response = await this.container.UpsertItemStreamAsync(stream, new PartitionKey(item.Year));
            }

            item.ETag = response.Headers["ETag"];

            return (item, response.StatusCode == System.Net.HttpStatusCode.Created);
        }

        /// <summary>Deletes the item.</summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            this.EnsureThatCosmosInitialized();

            ItemResponse<Movie> itemResponse = await this.container.DeleteItemAsync<Movie>(id, new PartitionKey(partitionKey));
        }

        /// <summary>Gets the item.</summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        public async Task<Movie> GetItemAsync(string id, string partitionKey)
        {
            this.EnsureThatCosmosInitialized();

            this.itemsMemoryCache.TryGetValue<Movie>(Movie.ComposeUniqueKey(id, partitionKey), out Movie movieItem);

            using (ResponseMessage streamedResponse = await this.container.ReadItemStreamAsync(
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
                    movieItem = await streamedResponse.Content.ToObject<Movie>();

                    this.itemsMemoryCache.Set(Movie.ComposeUniqueKey(id, partitionKey), movieItem, new MemoryCacheEntryOptions { Size = 1 });
                }
                else
                {
                    movieItem = null;
                }
            }

            return movieItem;
        }

        /// <summary>Gets all items.</summary>
        public async Task<IEnumerable<Movie>> GetAllItemsAsync()
        {
            this.EnsureThatCosmosInitialized();

            var sqlQueryText = "SELECT * FROM c";

            var movies = new List<List<Movie>>();

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using (FeedIterator queryResultSetStreamIterator = this.container.GetItemQueryStreamIterator(queryDefinition))
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
                                .ToObject<List<Movie>>();

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

            return movies.SelectMany(x => x);
        }

        /// <summary>Determines whether CosmosDb repo is being initializing.</summary>
        public bool IsInitializing()
        {
            if( !this.eventInitialized.WaitOne(0))
            {
                return true;
            }

            return false;
        }

        /// <summary>Initializes CosmosDb.</summary>
        private async Task Initialize()
        {
            try
            {
                await this.CreateDatabaseAsync();
                await this.CreateContainerAsync();
                await this.ScaleContainerAsync();
            }
            catch (Exception exc)
            {
                this.initializationError = exc.Message;
            }

            this.eventInitialized.Set();
        }

        /// <summary>Waits for initialization.</summary>
        private void EnsureThatCosmosInitialized()
        {
            this.eventInitialized.WaitOne();

            if (this.initializationError != null)
            {
                throw new Exception(this.initializationError);
            }
        }

        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
        }

        /// <summary>
        /// Create the container if it does not exist. 
        /// </summary>
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(ContainerId, "/Year", 400);
        }

        /// <summary>
        /// Scale the throughput provisioned on an existing Container.
        /// You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
        /// </summary>
        private async Task ScaleContainerAsync()
        {
            // Read the current throughput
            int? throughput = await this.container.ReadThroughputAsync();
            if (throughput.HasValue)
            {
                int newThroughput = throughput.Value + 100;
                // Update throughput
                await this.container.ReplaceThroughputAsync(newThroughput);
            }
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService">IHostedService</see> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">
        /// Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)">StopAsync(System.Threading.CancellationToken)</see> is called.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task">Task</see> that represents the long running operations.</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return this.Initialize();
        }

        /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.cosmosClient != null)
            {
                this.cosmosClient.Dispose();
            }

            return base.StopAsync(cancellationToken);
        }
    }
}
