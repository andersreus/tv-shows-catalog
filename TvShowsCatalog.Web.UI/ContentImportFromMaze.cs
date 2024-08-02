using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace TvShowsCatalog.Web.UI
{
    public class ContentImportFromMaze : IRecurringBackgroundJob
    {
        // Increase to 60 minutes when ImportContentService is done
        public TimeSpan Period { get => TimeSpan.FromMinutes(1); }

        TimeSpan Delay = TimeSpan.FromSeconds(5);

        // By default the job is only running on one server. So no need for configuring serverroles.

        // No-op event as the period never changes on this job
        public event EventHandler PeriodChanged { add { } remove { } }

        private readonly IImportContentService _importContentService;

        public ContentImportFromMaze(IImportContentService importContentService)
        {
            _importContentService = importContentService;
        }

        public async Task RunJobAsync()
        {
            // Call importcontent, pass in the unique identifier for the contenttype (parentId used in the IContentService create method)
            // Right now parentKey is hardcorded to -1 to insert all content as rootnodes.

            // TODO: Do logic that checks if there is content already and if it needs to be updated. This will live in the ImportContentService, check that
            await _importContentService.ImportContentAsync(-1);
        }
    }
}
