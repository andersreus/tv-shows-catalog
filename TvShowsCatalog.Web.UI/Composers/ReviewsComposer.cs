using TvShowsCatalog.Web.UI.NotificationHandlers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;

namespace TvShowsCatalog.Web.UI.Composers
{
    public class ReviewsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder) => builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification,RunReviewsMigration>();
    }
}
