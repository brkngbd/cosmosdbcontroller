namespace CosmosDbController
{
    /// <summary>
    ///   Cosmos DB settings
    /// </summary>
    public class CosmosDbOptions
    {
        public const string CosmosDb = "CosmosDb";

        /// <summary>Gets or sets the container identifier.</summary>
        /// <value>The container identifier.</value>
        public string ContainerId { get; set; }

        /// <summary>Gets or sets the database identifier.</summary>
        /// <value>The database identifier.</value>
        public string DatabaseId { get; set; }

        /// <summary>Gets or sets the endpoint URI.</summary>
        /// <value>The endpoint URI.</value>
        public string EndpointUri { get; set; }
        
        /// <summary>Gets or sets the primary key.</summary>
        /// <value>The primary key.</value>
        public string PrimaryKey { get; set; }
    }
}
