using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TvShowsCatalog.Web.Models.ApiModels
{
    public class TvMazeModel
    {
        // GET - https://api.tvmaze.com/shows?page=0
        // TODO: Add more properties based on postman response. Image, genre etc

        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
