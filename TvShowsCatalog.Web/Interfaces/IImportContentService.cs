using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using TvShowsCatalog.Web.Helpers;
using TvShowsCatalog.Web.Models.ApiModels;
using Umbraco.Cms.Core.Models;

namespace TvShowsCatalog.Web.Services
{
    public interface IImportContentService
    {
        Task ImportContentAsync(Guid? allTvShowsContentNodeKey);
        Task CreateContent(TvMazeModel tvshow, IMedia media, Guid? allTvShowsContentNodeKey, string[] cultures, IContentType tvShowContentType);
        ImportDecision ShouldRunImport();
        Task ImportFirstBatchAsync(Guid? allTvShowsContentNodeKey);
	}
}
