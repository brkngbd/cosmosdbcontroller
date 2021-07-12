using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Storage.Core.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace cosmosdbcontroller
{
    /// <summary>
    /// Used to communicate to CosmosDB service.
    /// </summary>
    public class CosmosDbrepository : BackgroundService, ICosmosDbrepository
    {
        /// <summary>The database identifier</summary>
        private const string databaseId = "db";

        /// <summary>The container identifier</summary>
        private const string containerId = "items";

        /// <summary>The Azure Cosmos DB endpoint URI for running this sample</summary>
        private readonly string endpointUri;

        /// <summary>The primary key for the Azure Cosmos account.</summary>
        private readonly string primaryKey;

        /// <summary>The Cosmos client instance</summary>
        private CosmosClient cosmosClient;

        /// <summary>The database we will create.</summary>
        private Database database;

        /// <summary>The container we will create.</summary>
        private Container container;

        private AsyncManualResetEvent eventInitialized = new AsyncManualResetEvent(false);

        private string initializationError = null;

        /// <summary>Initializes a new instance of the <see cref="CosmosDbrepository" /> class.</summary>
        /// <param name="configuration">The configuration.</param>
        public CosmosDbrepository(IConfiguration configuration)
        {
            endpointUri = configuration["EndpointUri"];
            primaryKey = configuration["PrimaryKey"];

            this.cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
        }

        /// <summary>Creates the item.</summary>
        /// <param name="item">The item to create.</param>
        public async Task<Movie> CreateItemAsync(Movie item)
        {
            await WaitForInitialization();

            ItemResponse<Movie> itemResponse = null;
            try
            {
                itemResponse = await this.container.ReadItemAsync<Movie>(item.id, new PartitionKey(item.Title));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                itemResponse = await this.container.CreateItemAsync<Movie>(item, new PartitionKey(item.Title));
            }
            return itemResponse;
        }

        /// <summary>Updates the item.</summary>
        /// <param name="id">The item identifier.</param>
        /// <param name="item">The item.</param>
        public async Task UpdateItemAsync(string id, Movie item)
        {
            await WaitForInitialization();

            ItemResponse<Movie> itemResponse = await this.container.ReadItemAsync<Movie>(id, new PartitionKey(item.Title));
            var itemBody = itemResponse.Resource;
            itemBody.ImdbRating = item.ImdbRating;
            await this.container.ReplaceItemAsync(itemBody, itemBody.id, new PartitionKey(itemBody.Title));
        }

        /// <summary>Deletes the item.</summary>
        /// <param name="id">The identifier.</param>
        public async Task DeleteItemAsync(string id)
        {
            await WaitForInitialization();

            Movie item = await GetItemAsync(id);
            if (item != null)
            {
                ItemResponse<Movie> itemResponse = await this.container.DeleteItemAsync<Movie>(id, new PartitionKey(item.Title));
            }
        }

        /// <summary>Gets the item.</summary>
        /// <param name="id">The identifier.</param>
        public async Task<Movie> GetItemAsync(string id)
        {
            await WaitForInitialization();

            var sqlQueryText = $"SELECT * FROM Movies m WHERE m.id='{id}'";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Movie> queryResultSetIterator = this.container.GetItemQueryIterator<Movie>(queryDefinition);

            Movie movie = null;

            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Movie> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                movie = currentResultSet.First();
            }

            return movie;
        }

        /// <summary>Gets all items.</summary>
        public async Task<IEnumerable<Movie>> GetAllItemsAsync()
        {
            await WaitForInitialization();

            var sqlQueryText = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Movie> queryResultSetIterator = this.container.GetItemQueryIterator<Movie>(queryDefinition);

            List<Movie> movies = new List<Movie>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Movie> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Movie movie in currentResultSet)
                {
                    movies.Add(movie);
                }
            }

            return movies;
        }

        private async Task Initialize()
        {
            try
            {
                await this.CreateDatabaseAsync();
                await this.CreateContainerAsync();
                await this.ScaleContainerAsync();
            }
            catch(Exception exc)
            {
                this.initializationError = exc.Message;
            }

            await eventInitialized.Set();
        }

        /// <summary>Waits for initialization.</summary>
        /// <exception cref="Exception"></exception>
        private async Task WaitForInitialization()
        {
            await eventInitialized.WaitAsync();

            if(this.initializationError!=null)
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
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        /// <summary>
        /// Create the container if it does not exist. 
        /// </summary>
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/Title", 400);
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
            return Initialize();
        }
    }
}
