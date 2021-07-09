using System.Collections.Generic;
using System.Threading.Tasks;

namespace cosmosdbcontroller
{
    /// <summary>
    /// Used to communicate to CosmosDB service.
    /// </summary>
    public interface ICosmosDbrepository
    {
        Task<Movie> CreateItemAsync(Movie item);
        Task DeleteItemAsync(string id);
        Task<IEnumerable<Movie>> GetAllItemsAsync();
        Task<Movie> GetItemAsync(string id);
        Task UpdateItemAsync(string id, Movie item);
    }
}