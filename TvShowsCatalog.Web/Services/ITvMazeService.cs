using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvShowsCatalog.Web.Models.ApiModels;

namespace TvShowsCatalog.Web.Services
{
    public interface ITvMazeService
    {
        Task<IEnumerable<TvMazeModel>> GetAllAsync();
    }
}
