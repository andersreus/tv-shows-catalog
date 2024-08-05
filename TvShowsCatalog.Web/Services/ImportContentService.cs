using System.ComponentModel;
using TvShowsCatalog.Web.Models.ApiModels;
using Umbraco.Cms.Core.Services;

namespace TvShowsCatalog.Web.Services
{
    public class ImportContentService : IImportContentService
    {
        private readonly IContentService _contentService;
        private readonly ITvMazeService _tvMazeService;

        public ImportContentService(IContentService contentService, ITvMazeService tVMazeService)
        {
            _contentService = contentService;
            _tvMazeService = tVMazeService;

        }

        // TODO: Task<IEnumerable<TvMazeModel>> UpdateImportedContent? In case there is new tv shows added to the list since the last import.

        public async Task<IEnumerable<TvMazeModel>> ImportContentAsync(int rootContentId)
        {
            var allTvShows = await _tvMazeService.GetAllAsync();

            var cultures = Array.Empty<string>();

            foreach (var show in allTvShows)
            {
                // TODO: Use publishedcontent instead, it's more efficient.
                // contentservice goes to the database, publishedcontent will use the cache.

                var tvshow = _contentService.Create($"{show.Name}", rootContentId, "tVShow");

                _contentService.Save(tvshow);
                _contentService.Publish(tvshow, cultures);
            }

            return allTvShows;
        }

        public async Task<bool> ShouldRunImport()
        {
            var rootContent = _contentService.GetRootContent().FirstOrDefault();

            // TODO add null check
            var rootContentId = rootContent.Id;
            
            bool isThereContent = _contentService.HasChildren(rootContentId);

            if (!isThereContent)
            {
                await ImportContentAsync(rootContentId);
            }
            return isThereContent;
        }
    }
}
