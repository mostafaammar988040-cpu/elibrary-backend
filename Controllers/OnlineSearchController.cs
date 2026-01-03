using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace eLibrary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnlineSearchController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public OnlineSearchController(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Search for books online using the Google Books API.
        /// Example: GET /api/OnlineSearch?q=harry+potter
        /// </summary>
        /// <param name="q">Search term (title, author, or keyword)</param>
        [HttpGet]
        public async Task<IActionResult> SearchBooks([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Please provide a search term.");

            // Read the API key from appsettings.json
            var apiKey = _config["GoogleBooks:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, "Google Books API key is missing.");

            // Construct the Google Books API URL
            var url = $"https://www.googleapis.com/books/v1/volumes?q={q}&key={apiKey}";

            // Call Google Books and return the JSON response directly
            var response = await _httpClient.GetStringAsync(url);
            return Content(response, "application/json");
        }
    }
}