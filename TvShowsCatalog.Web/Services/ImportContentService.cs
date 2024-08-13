using System.ComponentModel;
using TvShowsCatalog.Web.Models.ApiModels;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace TvShowsCatalog.Web.Services
{
    public class ImportContentService : IImportContentService
    {
        private readonly IContentService _contentService;
        private readonly ITvMazeService _tvMazeService;
        private readonly IImportMediaService _importMediaService;
        private readonly ICoreScopeProvider _coreScopeProvider;

        public ImportContentService(IContentService contentService, ITvMazeService tVMazeService, IImportMediaService importMediaService, ICoreScopeProvider coreScopeProvider)
        {
            _contentService = contentService;
            _tvMazeService = tVMazeService;
            _importMediaService = importMediaService;
            _coreScopeProvider = coreScopeProvider;
		}

		public void CreateContent(TvMazeModel tvshow, int rootContentId, string[] cultures)
		{
			var node = _contentService.Create($"{tvshow.Name}", rootContentId, "tVShow");
			node.SetValue("showSummary", $"{tvshow.Summary}");
			// SetValue -> mediapicker image

			_contentService.Save(node);
			_contentService.Publish(node, cultures);
		}

		// TODO: Check for updated/added tvshows?

		public IEnumerable<TvMazeModel> ImportContent(int rootContentId)
        {
            var allTvShows = _tvMazeService.GetAllAsync().GetAwaiter().GetResult();
            var cultures = Array.Empty<string>();

			using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
            _importMediaService.ImportBulkMedia(allTvShows);
			foreach (var show in allTvShows)
            {
                CreateContent(show, rootContentId, cultures);
			}
			scope.Complete();

			return allTvShows;
        }

        public (bool, int) ShouldRunImport()
        {
            var rootContent = _contentService.GetRootContent().FirstOrDefault();

            if (rootContent == null)
            {
                throw new Exception("Root content node was not found");
            }
            var rootContentId = rootContent.Id;
            
            bool isThereContent = _contentService.HasChildren(rootContentId);

            return (isThereContent,rootContentId);
        }
    }
}
