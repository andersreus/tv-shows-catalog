using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvShowsCatalog.Web.Models.CoreModels
{
    public class Review
    {
        public int Id { get; set; }
        public Guid TvShowUmbracoKey { get; set; }
        public Guid MemberUmbracoKey { get; set; }
        public required int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
    }
}
