using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TvShowsCatalog.Web.Models.ApiModels
{
    public class TvMazeImageModel
    {
        [JsonPropertyName("medium")]
        public string Medium {  get; set; }
        [JsonPropertyName("original")]
        public string Original {  get; set; }

    }
}
