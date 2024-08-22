using Microsoft.EntityFrameworkCore;
using TvShowsCatalog.Web.UI.Data;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace TvShowsCatalog.Web.UI.NotificationHandlers
{
    public class RunReviewsMigration : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
    {
        private readonly ReviewContext _reviewContext;

        public RunReviewsMigration(ReviewContext reviewContext)
        {
            _reviewContext = reviewContext;
        }
        public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
        {
            IEnumerable<string> pendingMigrations = await _reviewContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                await _reviewContext.Database.MigrateAsync();
            }
        }
    }
}
