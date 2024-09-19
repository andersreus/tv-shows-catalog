using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using TvShowsCatalog.Web.Services;
using TvShowsCatalog.Web.ViewModels;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace TvShowsCatalog.Web.UI.Controllers
{
    public class TvShowController : RenderController
    {
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;
        private readonly IReviewService _reviewService;
        public TvShowController
            (ILogger<TvShowController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IVariationContextAccessor variationContextAccessor,
            ServiceContext context,
            IReviewService reviewService)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
            _reviewService = reviewService;
        }

        public override IActionResult Index()
        {
            var reviews = _reviewService.GetReviews(CurrentPage.Key).ToList();
            var tvShowReviewViewModel = new TvShowReviewViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                Reviews = reviews
            };

            return CurrentTemplate(tvShowReviewViewModel);
        }
    }
}
