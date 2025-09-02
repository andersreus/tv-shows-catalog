using Serilog.Core;
using System.Diagnostics;
using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace TvShowsCatalog.Web.UI
{
    public class ContentImportFromMaze : IRecurringBackgroundJob
    {
        private readonly IImportContentService _importContentService;
        private readonly ILogger<ContentImportFromMaze> _logger;
        public TimeSpan Period { get => TimeSpan.FromDays(1); }
        private readonly TimeSpan Delay = TimeSpan.FromSeconds(20);

        // No-op (no operation = not used in this implementation) but it's required by the interface.
        public event EventHandler PeriodChanged { add { } remove { } }

        public ContentImportFromMaze(IImportContentService importContentService, ILogger<ContentImportFromMaze> logger)
        {
            _importContentService = importContentService;
            _logger = logger;
        }

        public async Task RunJobAsync()
        {
            var stopwatch = new Stopwatch();
            _logger.LogInformation("Content import job started at {StartTime}", DateTime.UtcNow);

            try
            {
                stopwatch.Start();
                var importDecision = _importContentService.ShouldRunImport();

                if (importDecision.ShouldRunImport)
                {
                    _logger.LogInformation("Running tvshow import for parent node with id {RootId}", importDecision.AllTvShowsContentNodeKey);
                    await _importContentService.ImportContentAsync(importDecision.AllTvShowsContentNodeKey);
                }
                else
                {
                    _logger.LogInformation("TvShows already exists under parent node with id {RootId}. Import was cancelled at {EndTime}", importDecision.AllTvShowsContentNodeKey, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to complete import job of tvshows from tvmaze Error: {Message}", ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Tvshow import job completed in {ElapsedTotalSeconds} ms at {EndTime}", stopwatch.Elapsed.TotalSeconds, DateTime.UtcNow);
            }
        }
    }
}
