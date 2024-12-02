using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using TvShowsCatalog.Web.Helpers;
using TvShowsCatalog.Web.Models.ApiModels;
using TvShowsCatalog.Web.Models.CoreModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
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
        private readonly IJsonSerializer _serializer;
        private readonly ILocalizedTextService _localizedTextService;

        public ImportContentService(IContentService contentService, ITvMazeService tVMazeService, IImportMediaService importMediaService, ICoreScopeProvider coreScopeProvider, IContentTypeService contentTypeService, ILogger<ImportContentService> logger, IJsonSerializer serializer, ILocalizedTextService localizedTextService)
        {
            _contentService = contentService;
            _tvMazeService = tVMazeService;
            _importMediaService = importMediaService;
            _coreScopeProvider = coreScopeProvider;
            _contentTypeService = contentTypeService;
            _logger = logger;
            _serializer = serializer;
            _localizedTextService = localizedTextService;
        }

        // TODO: CreateOrUpdateContent?
        public void CreateContent(TvMazeModel tvshow, IMedia media, int allTvShowsContentNodeId, string[] cultures, IContentType tvShowContentType)
		{
			var tvShowNode = _contentService.Create(tvshow.Name, allTvShowsContentNodeId, "tVShow");
			tvShowNode.SetValue("showSummary", tvshow.Summary ?? "No summary for this TV show");

            if (media == null)
            {
                _logger.LogWarning($"Could not find a image for this TV show: {tvshow.Name}");
            }
            tvShowNode.SetValue("showImage", media.GetUdi().ToString());
            
            // var template = tvShowContentType.AllowedTemplates.FirstOrDefault(t => t.Alias == "show");
            // if (template != null)
            // {
            //     tvShowNode.TemplateId = template.Id;
            // }
            // else
            // {
            //     _logger.LogWarning($"Could not find show template for this TV show, {tvshow.Name}");
            // }
            
            // Get the genre element type
            IContentType? elementContentType = _contentTypeService.Get("genre");

            foreach (var culture in cultures)
            {
                var elementData = new List<BlockItemData>();

                for (int index = 0; index < tvshow.Genres.Count(); index++)
                {
                    var genre = tvshow.Genres[index];
                    
                    // Localize needs a CultureInfo object and not just the culture string.
                    CultureInfo cultureInfo = new CultureInfo(culture);
                    
                    // Fejler her
                    var localizedGenreTitle = _localizedTextService.Localize("genres", genre, cultureInfo);
                    //var localizedGenreTitle = _localizedTextService.Localize("actions", genre, cultureInfo, null);

                    if (string.IsNullOrWhiteSpace(localizedGenreTitle))
                    {
                        localizedGenreTitle = genre;
                    }
                    elementData.Add(new(new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid()), elementContentType.Key, elementContentType.Alias)
                    {
                        // Set property values for the BlockItemData
                        // I assume Dictionary is used because the object can contain different datatypes for each property (string, int, bool etc)
                        RawPropertyValues = new Dictionary<string, object?>
                        {
                            {"title", localizedGenreTitle},
                            {"indexNumber", index.ToString()}
                        }
                    });
                }
            
                var contentUdi = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
                var blockListValue = new BlockListValue
                {
                    Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                    {
                        {
                            Constants.PropertyEditors.Aliases.BlockList,
                            new IBlockLayoutItem[]
                            {
                                new BlockListLayoutItem { ContentUdi = contentUdi }
                            }
                        }
                    },
                    ContentData = elementData
                };

                // Serialize the blocklist property (data and layout) to json
                var propertyValue = _serializer.Serialize(blockListValue);
                tvShowNode.SetValue("genres", propertyValue);
            }

            _contentService.Save(tvShowNode);
			_contentService.Publish(tvShowNode, cultures);
		}

		public async Task<IEnumerable<TvMazeModel>> ImportContentAsync(int allTvShowsContentNodeId, int importAmount)
        {
            var allTvShows = await _tvMazeService.GetAllAsync();
            var cultures = new[] { "en-US", "da-DK" };
            var tvShowContentType = _contentTypeService.Get("tvShow");

            // Break down alltvshows into manageable batches
            const int batchSize = 500;
            // Tracks current batch
            var currentIndex = 0;

            while (currentIndex < importAmount)
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
