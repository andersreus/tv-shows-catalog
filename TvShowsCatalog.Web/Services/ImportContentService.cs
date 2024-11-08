using Microsoft.Extensions.Logging;
using TvShowsCatalog.Web.Helpers;
using TvShowsCatalog.Web.Models.ApiModels;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using static System.Formats.Asn1.AsnWriter;

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
            
            List<BlockListModel> genreBlocks = new List<BlockListModel>();

            foreach (var block in genreBlocks)
            {
                node.SetValue("genres",tvshow.Genres);
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

		public async Task<IEnumerable<TvMazeModel>> ImportContentAsync(int allTvShowsContentNodeId)
        {
            var allTvShows = await _tvMazeService.GetAllAsync();
            var cultures = Array.Empty<string>();
            var tvShowContentType = _contentTypeService.Get("tvShow");

            // Break down alltvshows into manageable batches
            const int batchSize = 500;
            var totalshows = allTvShows.Count();
            // Tracks current batch
            var currentIndex = 0;

            while (currentIndex < totalshows)
            {
                // Bypass currentIndex and return the next batchsize to the list "batch"
                var batch = allTvShows.Skip(currentIndex).Take(batchSize).ToList();
                using (ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true)) // new scope/transaction pr batch
                {
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
                    scope.Complete(); // Transaction completed = save all changes for that batch to the db.
                }
                // Add batchSize(500) to the currentIndex
                currentIndex += batchSize;
                // 100ms - to avoid overwhelming system, might not be needed.
                await Task.Delay(100); 
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
