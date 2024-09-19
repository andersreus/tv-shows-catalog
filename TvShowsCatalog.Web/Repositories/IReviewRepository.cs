using Microsoft.AspNetCore.Mvc;
using TvShowsCatalog.Web.Models.CoreModels;
using TvShowsCatalog.Web.ViewModels;

namespace TvShowsCatalog.Web.Repositories
{
    public interface IReviewRepository
    {
        IEnumerable<Review> GetReviews(Guid umbracoNodeId);

        void AddReview(Review review);
    }
}
