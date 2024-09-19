using TvShowsCatalog.Web.Data;
using TvShowsCatalog.Web.Repositories;
using TvShowsCatalog.Web.Services;
using TvShowsCatalog.Web.UI.NotificationHandlers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace TvShowsCatalog.Web.UI.Composers
{
    public class ReviewsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, RunReviewsMigration>();
            builder.Services.AddTransient<IReviewService, ReviewService>();
            builder.Services.AddTransient<IReviewRepository, ReviewRepository>();
        }
    }
}