using System.ComponentModel;
using TvShowsCatalog.Web.Models.ApiModels;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

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

        // pass in IMedia object
		public void CreateContent(TvMazeModel tvshow, IMedia media, int rootContentId, string[] cultures)
		{
            // need tvshows node id, have homepage now as well
			var node = _contentService.Create($"{tvshow.Name}", rootContentId, "tVShow");
			node.SetValue("showSummary", $"{tvshow.Summary}");

            if (media != null)
            {
                node.SetValue("showImage", media.GetUdi().ToString());
            }

			_contentService.Save(node);
			_contentService.Publish(node, cultures);
		}

		// TODO: Check for updated/added tvshows?

		public IEnumerable<TvMazeModel> ImportContent(int rootContentId)
        {
            var allTvShows = _tvMazeService.GetAllAsync().GetAwaiter().GetResult();
            var cultures = Array.Empty<string>();

			using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

			foreach (var show in allTvShows)
            {
				var media = _importMediaService.ImportMediaAsync(show).GetAwaiter().GetResult();
                // Update rootContentId for the tvshows node id
				CreateContent(show, media, rootContentId, cultures);
			}
			scope.Complete();

			return allTvShows;
        }

        public (bool, int) ShouldRunImport()
        {
            // Refactor, needs to check for tvshows node under homepage node
            var rootContent = _contentService.GetRootContent().FirstOrDefault();

            if (rootContent == null)
            {
                throw new Exception("Root content node was not found");
            }
            // node id for tv shows
            var rootContentId = rootContent.Id;
            
            // has tv shows page any children?
            bool isThereContent = _contentService.HasChildren(rootContentId);

            return (isThereContent,rootContentId);
        }
    }
}
