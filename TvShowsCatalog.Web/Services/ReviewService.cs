using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvShowsCatalog.Web.Models.CoreModels;
using TvShowsCatalog.Web.Repositories;

namespace TvShowsCatalog.Web.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }
        public IEnumerable<Review> GetReviews(Guid umbracoNodeKey)
        {
            var reviewsOfTvshow = _reviewRepository.GetReviews(umbracoNodeKey);
            return reviewsOfTvshow;
        }

        public void AddReview(Review review)
        {
            _reviewRepository.AddReview(review);
        }
    }
}
