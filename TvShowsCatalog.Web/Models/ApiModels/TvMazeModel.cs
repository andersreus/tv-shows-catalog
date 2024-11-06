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
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }
        [JsonPropertyName("image")]
        public TvMazeImageModel? Image { get; set; }
        [JsonPropertyName("genres")]
        public string[] Genres { get; set; }
    }
}
