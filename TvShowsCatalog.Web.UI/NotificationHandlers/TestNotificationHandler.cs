using System.Globalization;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace TvShowsCatalog.Web.UI.NotificationHandlers;

public class TestNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly ILocalizedTextService _localizedTextService;
    
    public TestNotificationHandler(ILocalizedTextService localizedTextService)
    {
        _localizedTextService = localizedTextService;
    }

    public Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        var localizedGenreTitle = _localizedTextService.Localize("genres", "Drama", CultureInfo.GetCultureInfo("en-US"));
        return Task.CompletedTask;
    }
}

public class MyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, TestNotificationHandler>();
    }
} 