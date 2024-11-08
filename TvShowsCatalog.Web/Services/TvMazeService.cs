using Microsoft.Extensions.Logging;
using System.Text.Json;
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

        public async Task<IEnumerable<TvMazeModel>> GetAllAsync()
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
        }
        
    }
}
