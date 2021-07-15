namespace CosmosDbController
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Used to communicate to CosmosDB service.
    /// </summary>
    public interface ICosmosDbrepository
    {
        Task<(MovieModel itemMovie, bool created)> CreateItemAsync(MovieModel item);
        Task DeleteItemAsync(string id, string partitionKey);
        Task<IEnumerable<MovieModel>> GetAllItemsAsync();
        Task<MovieModel> GetItemAsync(string id, string partitionKey);
        Task<(MovieModel itemMovie, bool created)> UpdateItemAsync(string id, MovieModel item);
    }
}