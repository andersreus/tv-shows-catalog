using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvShowsCatalog.Web.Models.ApiModels;
using Umbraco.Cms.Core.Models;

namespace TvShowsCatalog.Web.Services
{
	public interface IImportMediaService
	{
		IMedia CreateMediaRootFolder();
		Task<IMedia> ImportMediaAsync(TvMazeModel tvshow);
		// IEnumerable<IMedia> ImportBulkMedia(IEnumerable<TvMazeModel> tvshows);
		string GetImageFileFormat(string url);
	}
}
