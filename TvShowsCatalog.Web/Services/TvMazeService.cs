using System;
using System.Collections.Generic;
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
        public async Task<IEnumerable<TvMazeModel>> GetAllAsync()
        {
            // TODO: Get all tv shows. No pagination. But wait until the end to pull all 60k tvshows.
            // Error handling, try catch?
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.tvmaze.com/shows?page=0");

            // Could also use httpclient instead of factory. Read factory is better formance wise and the code is the same anyway.
            var client = HttpClientFactory.Create();

            var response = await client.SendAsync(request);

            // Can't use ReadAsStringAsync? Throw an error on the jsonString when deserializing it below. Using ReadAsStreamAsync instead.
            var jsonString = await response.Content.ReadAsStreamAsync();

            var allTvShows = await JsonSerializer.DeserializeAsync<List<TvMazeModel>>(jsonString);

            return allTvShows;
        }
    }
}
