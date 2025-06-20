﻿using Serilog.Core;
using System.Diagnostics;
using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace TvShowsCatalog.Web.UI
{
    public class ContentImportFromMaze : IRecurringBackgroundJob
    {
        private readonly IImportContentService _importContentService;
        private readonly ILogger<ContentImportFromMaze> _logger;
        // Run initial import 30 seconds after application startup. After that, once pr day.
        public TimeSpan Period { get => TimeSpan.FromDays(1); }
        
        TimeSpan Delay = TimeSpan.FromSeconds(30);

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
                    _logger.LogInformation("Running tvshow import for parent node with id {RootId}", importDecision.AllTvShowsContentNodeId);
                    
                    // Current amount is 85135 (2 june 2025)
                    var importedItems = await _importContentService.ImportContentAsync(importDecision.AllTvShowsContentNodeId,500);

                    _logger.LogInformation("Imported {Count} tvshows at {Time}", importedItems.Count(), DateTime.UtcNow);
                }
                else
                {
                    _logger.LogInformation("TvShows already exists under parent node with id {RootId}. Import was cancelled at {EndTime}", importDecision.AllTvShowsContentNodeId, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to complete import job of tvshows from tvmaze Error: {Message}", ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                // No ElapsedSeconds?
                _logger.LogInformation("Tvshow import job completed in {ElapsedMilliseconds} ms at {EndTime}", stopwatch.ElapsedMilliseconds, DateTime.UtcNow);
            }
        }
    }
}
