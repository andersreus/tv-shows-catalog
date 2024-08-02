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
        // TODO: bool ShouldRunImport method? If yes, import all or new tv shows that has been added since last time.
        // TODO: Task<IEnumerable<TvMazeModel>> UpdateImportedContent? In case there is new tv shows added.

        public async Task<IEnumerable<TvMazeModel>> ImportContentAsync(int parentKey)
        {
            var allTvShows = await _tvMazeService.GetAllAsync();

            var cultures = Array.Empty<string>();

            foreach (var show in allTvShows)
            {
                // TODO: Use publishedcontent instead, it's more efficient.
                // contentservice goes to the database, publishedcontent will use the cache.

                // TODO: Check if content is already imported. This should not matter if I use Hangfire as Tony mentioned tho. Then it will just update on every hour instead.
                var tvshow = _contentService.Create($"{show.Name}", parentKey, "tVShow");

                _contentService.Save(tvshow);
                _contentService.Publish(tvshow, cultures);
            }

            return allTvShows;
        }
    }
}
