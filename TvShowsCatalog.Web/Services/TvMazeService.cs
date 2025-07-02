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

        /*public async Task<IEnumerable<TvMazeModel>> GetAllAsync()
        {
            var allTvShows = new List<TvMazeModel>();
            int page = 0;
            
            try
            {
                bool isThereTvShows = true;
                while (isThereTvShows)
                {
                    var client = _httpClientFactory.CreateClient();
                    var url = $"https://api.tvmaze.com/shows?page={page}";
                    var response = await client.GetAsync(url);
        
                    response.EnsureSuccessStatusCode();
        
                    var jsonString = await response.Content.ReadAsStreamAsync();
                    var tvShows = await JsonSerializer.DeserializeAsync<List<TvMazeModel>>(jsonString);
        
                    isThereTvShows = tvShows.Any();
        
                    allTvShows.AddRange(tvShows);
        
                    page++;
        
                    // 100ms
                    //await Task.Delay(100);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Request error doing tvmaze api fetch: ", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError("JSON deserialization error doing tvmaze api fetch: ", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured doing tvmaze api fetch.", ex);
            }
        
            return allTvShows;
        }*/


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
