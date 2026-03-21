using Learnova.Data;
using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnova.Services.Implementation
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseReview>> GetCourseReviewsAsync(int courseId)
        {
            return await _context.CourseReviews
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<CourseReview> AddReviewAsync(CourseReview review)
        {
            _context.CourseReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<double> GetAverageRatingAsync(int courseId)
        {
            var reviews = await _context.CourseReviews
                .Where(r => r.CourseId == courseId)
                .ToListAsync();

            if (!reviews.Any()) return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<bool> HasUserReviewedAsync(string userId, int courseId)
        {
            return await _context.CourseReviews
                .AnyAsync(r => r.UserId == userId && r.CourseId == courseId);
        }
    }
}