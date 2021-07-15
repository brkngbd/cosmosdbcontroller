namespace CosmosDbController
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;

    public class CosmosDbInitializer
    {
        /// <summary>The Cosmos client instance</summary>
        private readonly CosmosClient cosmosClient;

        /// <summary>The database identifier</summary>
        private readonly string DatabaseId;

        /// <summary>The container identifier</summary>
        private readonly string ContainerId;

        /// <summary>The database we will create.</summary>
        private Database database;

        /// <summary>The container we will create.</summary>
        private Container container;

        public CosmosDbInitializer(CosmosClient cosmosClient, IConfiguration configuration)
        {
            this.DatabaseId = configuration["Databaseid"];
            this.ContainerId = configuration["ContainerId"];
            this.cosmosClient = cosmosClient;
        }

        /// <summary>Initializes CosmosDb.</summary>
        public async Task Initialize()
        {
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.ScaleContainerAsync();
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
    }
}
