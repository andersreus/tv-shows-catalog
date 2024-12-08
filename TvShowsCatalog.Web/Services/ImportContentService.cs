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

        public ImportContentService(IContentService contentService, ITvMazeService tVMazeService, IImportMediaService importMediaService, ICoreScopeProvider coreScopeProvider, IContentTypeService contentTypeService, ILogger<ImportContentService> logger, IJsonSerializer serializer, ITranslationService translationService)
        {
            _contentService = contentService;
            _tvMazeService = tVMazeService;
            _importMediaService = importMediaService;
            _coreScopeProvider = coreScopeProvider;
            _contentTypeService = contentTypeService;
            _logger = logger;
            _serializer = serializer;
            _translationService = translationService;
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
            
            // Get the genre content/element type for the genre blocks
            IContentType? elementContentType = _contentTypeService.Get("genre");
            
            // Layout and ContentData was not correctly linked, was using two sets of UDI's.
            
            // Iterate over each culture for making block variations.
            foreach (var culture in cultures)
            {
                // Create lists to represent the elementdata (contentData) and the layout items
                var elementData = new List<BlockItemData>();
                var layoutItems = new List<IBlockLayoutItem>();
                
                // Check if culture is default = en-US
                var isDefaultCulture = string.Equals(culture, "en-US", StringComparison.OrdinalIgnoreCase);
                
                // Loop through each genre attached to the tvshow
                for (int index = 0; index < tvshow.Genres.Count(); index++)
                {
                    // Save current index in local variable
                    var genre = tvshow.Genres[index];
                    // If default culture, keep it as it is. If not, call translationservice to get the translated version (da-DK)
                    var variantGenreTitle = isDefaultCulture ? genre : _translationService.GetTranslation(culture,genre);
                    // Create a unique identifier for the current block
                    var contentUdi = new GuidUdi(Constants.UdiEntityType.Element, Guid.NewGuid());
                    // Define and set properties for current block
                    elementData.Add(new(contentUdi, elementContentType.Key, elementContentType.Alias)
                    {
                        RawPropertyValues = new Dictionary<string, object?>
                        {
                            {"title", variantGenreTitle},
                            {"indexNumber", index.ToString()}
                        }
                    });
                    // Create the link between the block and the layout. The layout represents the visual arrangement of the blocks (check json).
                    layoutItems.Add(new BlockListLayoutItem
                    {
                        // Same UDI as for the content of the block
                        ContentUdi = contentUdi
                    });
                }
                // Create the blocklist value object
                var blockListValue = new BlockListValue
                {
                    // Assign the layout with reference to each blocks UDI
                    Layout = new Dictionary<string, IEnumerable<IBlockLayoutItem>>
                    {
                        {Constants.PropertyEditors.Aliases.BlockList, layoutItems}
                    },
                    // Set the content data
                    ContentData = elementData
                };
                // Serialize the blocklist value object into json format
                var propertyValue = _serializer.Serialize(blockListValue);
                // assign the blocklist to my tvshow
                tvShowNode.SetValue("genres", propertyValue, culture);
            }
            // Names of each variant needs to be set. Just give it the default tvshow name for both variants.
            tvShowNode.SetCultureName(tvshow.Name,"en-US");
            tvShowNode.SetCultureName(tvshow.Name,"da-DK");

            _contentService.Save(tvShowNode);
            // Publish with all cultures
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
