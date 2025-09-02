using System.Globalization;
using System.Reflection.Metadata;
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
using Umbraco.Cms.Core.Models.ContentEditing;
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
        private const string TvShowGenrePropertyAlias = "genres";

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
        
        public async Task CreateContent(TvMazeModel tvshow, IMedia media, Guid? allTvShowsContentNodeKey, string[] cultures, IContentType tvShowContentType)
        {
            if (tvshow == null)
            {
                return;
            }

            var listOfProperties = new List<PropertyValueModel>();
            try
            {
                listOfProperties.Add(new PropertyValueModel
                {
                    Alias = TvShowSummaryPropertyAlias,
                    Value = !string.IsNullOrEmpty(tvshow.Summary) ? tvshow.Summary : "No summary available"
                });
                listOfProperties.Add(new PropertyValueModel
                {
                    Alias = TvShowImagePropertyAlias,
                    Value = media != null ? media.GetUdi().ToString() : string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding properties for TV show {TvShowName}. Error: {Message}", tvshow.Name, ex.Message);
            }

            var listOfVariants = new List<VariantModel>();
            IContentType? elementContentType = _contentTypeService.Get(GenreContentTypeAlias);

            foreach (var culture in cultures)
            {
                try
                {
                    var currentVariant = new VariantModel
                    {
                        Culture = culture,
                        Name = tvshow.Name
                    };
                    listOfVariants.Add(currentVariant);
                
                    var elementData = new List<BlockItemData>();
                    var layoutItems = new List<IBlockLayoutItem>();
                    var isDefaultCulture = string.Equals(culture, "en-US", StringComparison.OrdinalIgnoreCase);

                    if (tvshow.Genres != null)
                    {
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
                    var blockValue = new PropertyValueModel
                    {
                        Alias = TvShowGenrePropertyAlias,
                        Value = propertyValue,
                        Culture = culture
                    };
                    listOfProperties.Add(blockValue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding variant for TV show {TvShowName} in culture {Culture}. Error: {Message}", tvshow.Name, culture, ex.Message);
                }
            }

            var documentKey = Guid.NewGuid();
            var createModel = new ContentCreateModel
            {
                Key = documentKey,
                ContentTypeKey = tvShowContentType.Key,
                ParentKey = allTvShowsContentNodeKey,
                Properties = listOfProperties,
                Variants = listOfVariants
            };
            
            var content = await _contentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

            if (content.Success is false)
            {
                _logger.LogCritical("Could not create content for TV show {TvShowName}. Status: {Errors}", tvshow.Name, content.Status);
            }

            await _contentPublishingService.PublishAsync(
                content.Result.Content.Key,
                [
                    new() { Culture = cultures[0] },
                    new() { Culture = cultures[1] },
                ],
                Constants.Security.SuperUserKey);
        }

        public async Task ImportContentAsync(Guid? allTvShowsContentNodeKey)
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
                            await CreateContent(show, media, allTvShowsContentNodeKey, cultures, tvShowContentType);
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

                    await Task.Delay(100);
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
            var allTvShowsContentNodeKey = allTvShowsContentNode.Key;
            
            bool isThereContent = _contentService.HasChildren(allTvShowsContentNodeId);
            
            return new ImportDecision
            {
                ShouldRunImport = !isThereContent,
                AllTvShowsContentNodeKey = allTvShowsContentNodeKey
            };
        }

        public async Task ImportFirstBatchAsync(Guid? allTvShowsContentNodeKey)
        {
            var cultures = new[] { "en-US", "da-DK" };
            var tvShowContentType = _contentTypeService.Get(TvShowContentTypeAlias);

            await foreach (var page in _tvMazeService.GetPageAsync())
            {
                var firstBatch = page.Take(100);
                using (ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true))
                {
                    foreach (var show in firstBatch)
                    {
                        var media = await _importMediaService.ImportMediaAsync(show);
                        await CreateContent(show, media, allTvShowsContentNodeKey, cultures, tvShowContentType);
                    }
                    scope.Complete();
                }
                break;
            }
        }
    }
}
