namespace CosmosDbController
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ICosmosDbrepository cosmosRepo;

        public MoviesController(ICosmosDbrepository cosmosDao)
        {
            this.cosmosRepo = cosmosDao;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return await ExecuteFunc(async () =>
            {
                return Ok(await cosmosRepo.GetAllItemsAsync());
            });
        }

        // GET api/movies/Terminator/1986
        [HttpGet("{id}/{partitionKey}")]
        public async Task<IActionResult> Get(string id, string partitionKey)
        {
            return await ExecuteFunc(async () =>
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }

                var item = await cosmosRepo.GetItemAsync(id, partitionKey);
                if (item == null)
                {
                    return NotFound();
                }

                return Ok(item);
            });
        }

        // POST api/movies
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Movie item)
        {
            return await ExecuteFunc(async () =>
            {
                if (item == null)
                {
                    return BadRequest();
                }

                (Movie itemMovie, bool created) creationResult = await cosmosRepo.CreateItemAsync(item);

                if (creationResult.created)
                {
                    return CreatedAtAction("GET", "movies", new Movie { Title = item.Title }, creationResult.itemMovie);
                }
                else
                {
                    return Ok(creationResult.itemMovie);
                }
            });
        }

        // PUT api/movies/Terminator/1986
        [HttpPut("{id}/{partitionKey}")]
        public async Task<IActionResult> Update(string id, string partitionKey, [FromBody] Movie item)
        {
            return await ExecuteFunc(async () =>
            {
                if (item == null || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(partitionKey))
                {
                    return BadRequest();
                }

                (Movie itemMovie, bool created) creationResult = await cosmosRepo.UpdateItemAsync(id, item);

                if (creationResult.created)
                {
                    return CreatedAtAction("GET", "movies", new Movie { Title = item.Title }, item);
                }
                else
                {
                    return Ok(item);
                }
            });
        }

        // DELETE api/movies/Terminator/1986
        [HttpDelete("{id}/{partitionKey}")]
        public async Task<IActionResult> Delete(string id, string partitionKey)
        {
            return await ExecuteFunc(async () =>
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(partitionKey))
                {
                    return BadRequest();
                }

                await cosmosRepo.DeleteItemAsync(id, partitionKey);

                return Ok();
            });
        }

        /// <summary>Used to warp execution of actions to handle error.</summary>
        /// <param name="action">The action.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private async Task<IActionResult> ExecuteFunc(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
