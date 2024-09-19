using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvShowsCatalog.Web.Models.CoreModels;

namespace TvShowsCatalog.Web.Services
{
    public interface IReviewService
    {
        IEnumerable<Review> GetReviews(Guid umbracoNodeKey);
        void AddReview(Review review);
    }
}
