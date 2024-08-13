using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvShowsCatalog.Web.Models.ApiModels;
using Umbraco.Cms.Core.Models;

namespace TvShowsCatalog.Web.Services
{
    public interface IImportContentService
    {
        Task<IEnumerable<TvMazeModel>> ImportContentAsync(int parentKey);
        Task<(bool, int)> ShouldRunImportAsync();

	}
}
