using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
                while (true) // Keep going until no more data can be fetched
                {
                    // Create request for current page on tvmaze
                    var client = _httpClientFactory.CreateClient();
                    var url = $"https://api.tvmaze.com/shows?page={page}";
                    var response = await client.GetAsync(url);

                    // Ensure the response is successful
                    response.EnsureSuccessStatusCode();

                    // Deserialize the response from json to a List of TVMazeModel
                    var jsonString = await response.Content.ReadAsStreamAsync();
                    var tvShows = await JsonSerializer.DeserializeAsync<List<TvMazeModel>>(jsonString);

                    // Break if no more shows are returned (no more pages with tv show data)
                    if (tvShows == null || !tvShows.Any())
                    {
                        break;
                    }

                    // Add the shows to the list
                    allTvShows.AddRange(tvShows);

                    // Continue to the next page
                    page++;

                    // 100ms
                    await Task.Delay(100);
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
