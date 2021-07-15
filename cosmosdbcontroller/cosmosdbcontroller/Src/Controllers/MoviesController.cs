namespace CosmosDbController
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    ///   The CRUD controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        /// <summary>The cosmos repo instance.</summary>
        private readonly CosmosDbrepository cosmosRepo;

        private readonly CosmosDbInitializerModel cosmosDbInitializerModel;

        /// <summary>Initializes a new instance of the <see cref="MoviesController" /> class.</summary>
        /// <param name="cosmosDao">The cosmos DAO.</param>
        public MoviesController(CosmosDbrepository cosmosDao, CosmosDbInitializerModel cosmosDbInitializerModel)
        {
            this.cosmosRepo = cosmosDao;
            this.cosmosDbInitializerModel = cosmosDbInitializerModel;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return await this.ExecuteFunc(async () =>
            {
                return Ok(await cosmosRepo.GetAllItemsAsync());
            });
        }

        // GET api/movies/Terminator/1986
        [HttpGet("{id}/{partitionKey}")]
        public async Task<IActionResult> Get(string id, string partitionKey)
        {
            return await this.ExecuteFunc(async () =>
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
        public async Task<IActionResult> Create([FromBody] MovieModel item)
        {
            return await this.ExecuteFunc(async () =>
            {
                if (item == null)
                {
                    return BadRequest();
                }

                (MovieModel itemMovie, bool created) creationResult = await cosmosRepo.CreateItemAsync(item);

                if (creationResult.created)
                {
                    return  CreatedAtAction(nameof(Get), new { id = item.Title, partitionKey = item.Year }, item);
                }
                else
                {
                    return Ok(creationResult.itemMovie);
                }
            });
        }

        // PUT api/movies/Terminator/1986
        [HttpPut("{id}/{partitionKey}")]
        public async Task<IActionResult> Update(string id, string partitionKey, [FromBody] MovieModel item)
        {
            return await this.ExecuteFunc(async () =>
            {
                if (item == null || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(partitionKey))
                {
                    return BadRequest();
                }

                (MovieModel itemMovie, bool created) creationResult = await cosmosRepo.UpdateItemAsync(id, item);

                if (creationResult.created)
                {
                    return CreatedAtAction("GET", "movies", new MovieModel { Title = item.Title }, item);
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
            return await this.ExecuteFunc(async () =>
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
            if (this.cosmosDbInitializerModel.InitTask==null || !this.cosmosDbInitializerModel.InitTask.IsCompleted)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Cosmos Db repository is initializing, try again later.");
            }

            try
            {
                return await action();
            }
            catch (CosmosException e) when ((int)e.StatusCode == StatusCodes.Status404NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
