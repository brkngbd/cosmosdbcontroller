using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace cosmosdbcontroller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ICosmosDbrepository cosmosDao;

        public MoviesController(ICosmosDbrepository cosmosDao)
        {
            this.cosmosDao = cosmosDao;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await cosmosDao.GetAllItemsAsync());
        }

        // GET api/movies/Terminator.1986
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var item = await cosmosDao.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // POST api/movies
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Movie item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            await cosmosDao.CreateItemAsync(item);

            return CreatedAtAction("GET", "movies", new Movie { id = item.id }, item);
        }

        // PUT api/movies/Terminator.1986
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Movie item)
        {
            var movieItem = await cosmosDao.GetItemAsync(id);
            if (movieItem == null)
            {
                return NotFound();
            }

            await cosmosDao.UpdateItemAsync(id, item);

            return Ok();
        }

        // DELETE api/movies/Terminator.1986
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await cosmosDao.DeleteItemAsync(id);

            return Ok();
        }
    }
}
