using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ImportContentService> _logger;

        public ImportContentService(IContentService contentService, ITvMazeService tVMazeService, IImportMediaService importMediaService, ICoreScopeProvider coreScopeProvider, IContentTypeService contentTypeService, ILogger<ImportContentService> logger)
        {
            _contentService = contentService;
            _tvMazeService = tVMazeService;
            _importMediaService = importMediaService;
            _coreScopeProvider = coreScopeProvider;
            _contentTypeService = contentTypeService;
            _logger = logger;
        }

        // TODO: CreateOrUpdateContent?
        public void CreateContent(TvMazeModel tvshow, IMedia media, int allTvShowsContentNodeId, string[] cultures, IContentType tvShowContentType)
		{
			var node = _contentService.Create($"{tvshow.Name}", allTvShowsContentNodeId, "tVShow");
			node.SetValue("showSummary", $"{tvshow.Summary}");

            if (media != null)
            {
                node.SetValue("showImage", media.GetUdi().ToString());
            }

            // Set template for each node.
            var template = tvShowContentType.AllowedTemplates.FirstOrDefault(t => t.Alias == "show");
            if (template != null)
            {
                node.TemplateId = template.Id;
            }

            _contentService.Save(node);
			_contentService.Publish(node, cultures);
		}

		public async Task<IEnumerable<TvMazeModel>> ImportContent(int allTvShowsContentNodeId)
        {
            var allTvShows = await _tvMazeService.GetAllAsync();
            var cultures = Array.Empty<string>();
            var tvShowContentType = _contentTypeService.Get("tvShow");

            const int batchSize = 1000;
            var totalshows = allTvShows.Count();
            var currentIndex = 0;

			using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

            while (currentIndex < totalshows)
            {
                var batch = allTvShows.Skip(currentIndex).Take(batchSize).ToList();
                foreach (var show in batch)
                {
                    try
                    {
                        var media = await _importMediaService.ImportMediaAsync(show);
                        CreateContent(show, media, allTvShowsContentNodeId, cultures, tvShowContentType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error importing TV show {show.Name}", ex.Message);
                    }
                }
                currentIndex += batchSize;
                scope.Complete(); // Complete the current batch transacion with db
            }
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
