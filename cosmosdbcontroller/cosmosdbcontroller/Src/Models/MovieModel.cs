namespace CosmosDbController
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///   Movie document item
    /// </summary>
    public class MovieModel
    {
        /// <summary>Gets or sets the e tag.</summary>
        /// <value>The _etag.</value>
        [JsonPropertyName("_etag")]
        public string ETag { get; set; }

        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public string id { get; set; }

        /// <summary>Gets or sets the title.</summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>Gets or sets the year.</summary>
        /// <value>The year.</value>
        public string Year { get; set; }  // in format "Movie year"

        /// <summary>Gets or sets the imdb rating.</summary>
        /// <value>The imdb rating of the movie.</value>
        public string ImdbRating { get; set; }

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>Gets the unique key.</summary>
        public string GetUniqueKey()
        {
            return ComposeUniqueKey(this.id, this.Year);
        }

        /// <summary>Composes the unique key.</summary>
        /// <param name="id">The identifier.</param>
        /// <param name="partitionKey">The partition key.</param>
        public static string ComposeUniqueKey(string id, string partitionKey)
        {
            return $"id={id}, partitionKey={partitionKey}";
        }
    }
}
