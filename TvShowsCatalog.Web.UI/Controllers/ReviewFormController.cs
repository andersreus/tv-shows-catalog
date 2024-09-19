using Microsoft.AspNetCore.Mvc;
using TvShowsCatalog.Web.Models.CoreModels;
using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace TvShowsCatalog.Web.UI.Controllers
{
    public class ReviewFormController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IReviewService _reviewService;
        private readonly IContentService _contentService;
        private readonly ILogger _logger;
        public ReviewFormController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IMemberManager memberManager, IReviewService reviewService, IContentService contentService, ILogger<ReviewFormController> logger) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _reviewService = reviewService;
            _contentService = contentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview([FromForm] Review review)
        {
            try
            {
                var member = await _memberManager.GetCurrentMemberAsync();
                if (member == null)
                {
                    return Unauthorized();
                }


                review.MemberUmbracoKey = member.Key;
                review.CreationDate = DateTime.Now;

                // Get umbracokey from form data and sets it on the review object
                if (Request.Form.TryGetValue("TvShowUmbracoKey", out var tvShowKey))
                {
                    review.TvShowUmbracoKey = Guid.Parse(tvShowKey);
                }

                _reviewService.AddReview(review);

                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add a review");
                return StatusCode(500, "Failed to process your request");
            }
        }
    }
}
