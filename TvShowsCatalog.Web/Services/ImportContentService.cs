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
        private readonly ITranslationService _translationService;
        private readonly IContentEditingService _contentEditingService;
        private readonly IContentPublishingService _contentPublishingService;
        private const string TvShowContentTypeAlias = "tVShow";
        private const string TvShowsContentTypeAlias = "tvShows";
        private const string GenreContentTypeAlias = "genre";
        private const string TvShowSummaryPropertyAlias = "showSummary";
        private const string TvShowImagePropertyAlias = "showImage";

        public ImportContentService(
            IContentService contentService, 
            ITvMazeService tVMazeService, 
            IImportMediaService importMediaService, 
            ICoreScopeProvider coreScopeProvider, 
            IContentTypeService contentTypeService, 
            ILogger<ImportContentService> logger, 
            IJsonSerializer serializer, 
            ITranslationService translationService,
            IContentEditingService contentEditingService,
            IContentPublishingService contentPublishingService)
        {
            _contentService = contentService;
            _tvMazeService = tVMazeService;
            _importMediaService = importMediaService;
            _coreScopeProvider = coreScopeProvider;
            _contentTypeService = contentTypeService;
            _logger = logger;
            _serializer = serializer;
            _translationService = translationService;
            _contentEditingService = contentEditingService;
            _contentPublishingService = contentPublishingService;
        }
        
        public async Task CreateContent(TvMazeModel tvshow, IMedia media, int allTvShowsContentNodeId, string[] cultures, IContentType tvShowContentType)
		{
			var tvShowNode = _contentService.Create(tvshow.Name, allTvShowsContentNodeId, TvShowContentTypeAlias);
			tvShowNode.SetValue(TvShowSummaryPropertyAlias, tvshow.Summary ?? "No summary for this TV show");

            if (media is null)
            {
                _logger.LogWarning($"Could not find a image for this TV show: {tvshow.Name}");
                tvShowNode.SetValue(TvShowImagePropertyAlias, null);
            }
            tvShowNode.SetValue(TvShowImagePropertyAlias, media.GetUdi().ToString());
            
            IContentType? elementContentType = _contentTypeService.Get(GenreContentTypeAlias);

            foreach (var culture in cultures)
            {
                var elementData = new List<BlockItemData>();
                var layoutItems = new List<IBlockLayoutItem>();
                var isDefaultCulture = string.Equals(culture, "en-US", StringComparison.OrdinalIgnoreCase);
                
                for (int index = 0; index < tvshow.Genres.Count(); index++)
                {
                    var genre = tvshow.Genres[index];
                    var variantGenreTitle = isDefaultCulture ? genre : _translationService.GetTranslation(culture,genre);
                    var contentUdi = new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid());
                    elementData.Add(new(contentUdi, elementContentType.Key, elementContentType.Alias)
                    {
                        RawPropertyValues = new Dictionary<string, object?>
                        {
                            {"title", variantGenreTitle},
                            {"indexNumber", index.ToString()}
                        }
                    });
                    
                    layoutItems.Add(new BlockListLayoutItem
                    {
                        ContentUdi = contentUdi
                    });
                }

                var blockListValue = new BlockListValue
                {
                    Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                    {
                        {Constants.PropertyEditors.Aliases.BlockList, layoutItems}
                    },
                    ContentData = elementData
                };
                var propertyValue = _serializer.Serialize(blockListValue);
                tvShowNode.SetValue("genres", propertyValue, culture);
            }

            foreach (var culture in cultures)
            {
                tvShowNode.SetCultureName(tvshow.Name,culture);
            }
            
            // Use IContentEditingService to save the content
            // Use IContentPublishingService to publish the content
            _contentService.Save(tvShowNode);
			_contentService.Publish(tvShowNode, cultures);
		}

        public async Task ImportContentAsync(int parentKey)
        {
            const int batchSize = 500;
            var cultures = new[] { "en-US", "da-DK" };
            var tvShowContentType = _contentTypeService.Get(TvShowContentTypeAlias);

            await foreach (var page in _tvMazeService.GetPageAsync())
            {
                foreach (var batch in page.Chunk(batchSize))
                {
                    using (ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true))
                    {
                        foreach (var show in batch)
                        {
                            var media = await _importMediaService.ImportMediaAsync(show);
                            CreateContent(show, media, parentKey, cultures, tvShowContentType);
                            // Run in debug mode, check if any exceptions are thrown and then add them in the catch block
                            /*try
                             {
                                 var media = await _importMediaService.ImportMediaAsync(show);
                                 CreateContent(show, media, parentKey, cultures, tvShowContentType);
                             }
                             catch ()
                             {
                                 _logger.LogError();
                             }*/
                        }

                        scope.Complete();
                    }

                    await Task.Delay(100); // Optional: avoid rate limits or DB load
                }
            }
        }

        public ImportDecision ShouldRunImport()
        {
            var allTvShowsContentNode = _contentService.GetRootContent().FirstOrDefault(c => c.ContentType.Alias == TvShowsContentTypeAlias);

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
