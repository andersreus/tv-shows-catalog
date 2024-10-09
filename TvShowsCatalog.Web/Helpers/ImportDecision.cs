using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvShowsCatalog.Web.Helpers
{
    public class ImportDecision
    {
        public bool ShouldRunImport { get; set; }
        public int AllTvShowsContentNodeId { get; set; }
    }
}
