using TvShowsCatalog.Web.Helpers;
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
        private readonly IContentTypeService _contentTypeService;

        public ImportContentService(IContentService contentService, ITvMazeService tVMazeService, IImportMediaService importMediaService, ICoreScopeProvider coreScopeProvider, IContentTypeService contentTypeService)
        {
            _contentService = contentService;
            _tvMazeService = tVMazeService;
            _importMediaService = importMediaService;
            _coreScopeProvider = coreScopeProvider;
            _contentTypeService = contentTypeService;
		}

        // pass in IMedia object
		public void CreateContent(TvMazeModel tvshow, IMedia media, int allTvShowsContentNodeId, string[] cultures, IContentType tvShowContentType)
		{
            // need tvshows node id, have homepage now as well
			var node = _contentService.Create($"{tvshow.Name}", allTvShowsContentNodeId, "tVShow");
			node.SetValue("showSummary", $"{tvshow.Summary}");

            if (media != null)
            {
                node.SetValue("showImage", media.GetUdi().ToString());
            }

            // Sets template for the nodes all can be rendered out of the box.
            var template = tvShowContentType.AllowedTemplates.FirstOrDefault(t => t.Alias == "show");
            if (template != null)
            {
                node.TemplateId = template.Id;
            }

            _contentService.Save(node);
			_contentService.Publish(node, cultures);
		}

		// TODO: Check for updated/added tvshows?

		public IEnumerable<TvMazeModel> ImportContent(int allTvShowsContentNodeId)
        {
            var allTvShows = _tvMazeService.GetAllAsync().GetAwaiter().GetResult();
            var cultures = Array.Empty<string>();
            var tvShowContentType = _contentTypeService.Get("tvShow");

			using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

			foreach (var show in allTvShows)
            {
				var media = _importMediaService.ImportMediaAsync(show).GetAwaiter().GetResult();
                // Update rootContentId for the tvshows node id
				CreateContent(show, media, allTvShowsContentNodeId, cultures, tvShowContentType);
			}
			scope.Complete();

			return allTvShows;
        }

        public ImportDecision ShouldRunImport()
        {
            var allTvShowsContentNode = _contentService.GetRootContent().FirstOrDefault(c => c.ContentType.Alias == "tvShows");

            if (allTvShowsContentNode == null)
            {
                throw new Exception("TV Shows root content node was not found");
            }

            var allTvShowsContentNodeId = allTvShowsContentNode.Id;
            
            bool isThereContent = _contentService.HasChildren(allTvShowsContentNodeId);

            return new ImportDecision
            {
                ShouldRunImport = !isThereContent,
                AllTvShowsContentNodeId = allTvShowsContentNodeId
            };
        }
    }
}
