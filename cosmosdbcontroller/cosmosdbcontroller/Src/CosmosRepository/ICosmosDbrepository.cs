namespace CosmosDbController
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Used to communicate to CosmosDB service.
    /// </summary>
    public interface ICosmosDbrepository
    {
        Task<(Movie itemMovie, bool created)> CreateItemAsync(Movie item);
        Task DeleteItemAsync(string id, string partitionKey);
        Task<IEnumerable<Movie>> GetAllItemsAsync();
        Task<Movie> GetItemAsync(string id, string partitionKey);
        Task<(Movie itemMovie, bool created)> UpdateItemAsync(string id, Movie item);

        bool IsInitializing();
    }
}