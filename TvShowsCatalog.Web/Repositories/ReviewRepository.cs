using Serilog.Context;
using TvShowsCatalog.Web.Data;
using TvShowsCatalog.Web.Models.CoreModels;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using static System.Formats.Asn1.AsnWriter;

namespace TvShowsCatalog.Web.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ReviewContext _reviewContext;

        public ReviewRepository(ReviewContext reviewContext)
        {
            _reviewContext = reviewContext;
        }
        public void AddReview(Review review)
        {
            _reviewContext.Add(review);
            _reviewContext.SaveChanges();
        }

        public IEnumerable<Review> GetReviews(Guid umbracoNodeId)
         {
            var tvShowReviews = _reviewContext.Reviews
                .Where(r => r.TvShowUmbracoKey.ToString() == umbracoNodeId.ToString())
                .ToList() ?? new List<Review>();
            return tvShowReviews;
        }
    }
}
