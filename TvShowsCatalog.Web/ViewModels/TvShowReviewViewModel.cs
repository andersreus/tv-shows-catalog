using TvShowsCatalog.Web.Models.ApiModels;
using TvShowsCatalog.Web.Models.CoreModels;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TvShowsCatalog.Web.ViewModels
{
    public class TvShowReviewViewModel : PublishedContentWrapped
    {
        // The PublishedContentWrapped accepts an IPublishedContent item as a constructor
        // So in this case I get my tvshow content from the IPublishedContent using the PublishedContentWrapped class.
        // In short = using the PublishedContentWrapped class, I maintain my original content (tvshow) and extend the functionality by bringing in my custom property = Reviews.
        public TvShowReviewViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
        }

        // Wrapping my custom table model to the viewmodel as it's not part of the PublishedContent.
        public List<Review> Reviews { get; set; }
        public Review PostReview { get; set; }
    }
}
