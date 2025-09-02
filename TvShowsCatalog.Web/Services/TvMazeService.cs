using Microsoft.Extensions.Logging;
using System.Text.Json;
using NPoco.fastJSON;
using TvShowsCatalog.Web.Models.ApiModels;

namespace TvShowsCatalog.Web.Services
{
    public class TvMazeService : ITvMazeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TvMazeService> _logger;

        public TvMazeService(IHttpClientFactory httpClientFactory, ILogger<TvMazeService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async IAsyncEnumerable<List<TvMazeModel>> GetPageAsync()
        {
            int currentPage = 0;
            while (true)
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://api.tvmaze.com/shows?page={currentPage}";
                
                List<TvMazeModel>? tvShowObjects = null;
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    
                    // Good name for variable. Debug and check value
                    var jsonStream = await response.Content.ReadAsStreamAsync();
                    tvShowObjects = await JsonSerializer.DeserializeAsync<List<TvMazeModel>>(jsonStream);

                    if (tvShowObjects is null || tvShowObjects.Count is 0)
                        yield break;
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error fetching page {currentPage} from the TvMaze api endpoint: {e.Message}");
                    yield break;
                }
                
                yield return tvShowObjects;
                currentPage++;
                await Task.Delay(100);
            }
        }
    }
}
