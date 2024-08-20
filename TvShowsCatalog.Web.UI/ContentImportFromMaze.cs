using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace TvShowsCatalog.Web.UI
{
    public class ContentImportFromMaze : IRecurringBackgroundJob
    {
        public TimeSpan Period { get => TimeSpan.FromMinutes(60); }

        TimeSpan Delay = TimeSpan.FromSeconds(1);

        public event EventHandler PeriodChanged { add { } remove { } }

        private readonly IImportContentService _importContentService;

        public ContentImportFromMaze(IImportContentService importContentService)
        {
            _importContentService = importContentService;
        }

        public async Task RunJobAsync()
        {
            // TODO: tuple = bad practice. Rethink
			var (shouldRunImport, allTvShowsContentNodeId) = _importContentService.ShouldRunImport();

            if (!shouldRunImport)
            {
                // Pass rootcontentId for tvshows node
				_importContentService.ImportContent(allTvShowsContentNodeId);
			}
        }
    }
}
