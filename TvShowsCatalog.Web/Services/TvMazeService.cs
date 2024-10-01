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

        public TvMazeService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Simplified version to just get the first page. No error handling, no nothing.

        public async Task<IEnumerable<TvMazeModel>> GetAllAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.tvmaze.com/shows?page=0");
            var client = HttpClientFactory.Create();
            var response = await client.SendAsync(request);
            var jsonString = await response.Content.ReadAsStreamAsync();
            var allTvShows = await JsonSerializer.DeserializeAsync<List<TvMazeModel>>(jsonString);
            return allTvShows;
        }

        // Build on the one below to iterate through all tvmaze pages and deserialize all tv shows.
        // Remember to inject ILogger

        /*
        public async Task<IEnumerable<TvMazeModel>> GetAllAsync()
        {
            var allTvShows = new List<TvMazeModel>();
            int page = 0;
            int maxPages = 10; // This is just for testing. Looping through all pages gives sql issues.

            try
            {
                //while (true)
                while (page < maxPages)
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
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Request error: ", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError("JSON deserialization error: ", ex);
            }

            return allTvShows;
        }
        */
        
    }
}
