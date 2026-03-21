using Learnova.Models.Entities;

namespace Learnova.Services.Interfaces
{
    public interface IReviewService
    {
        Task<List<CourseReview>> GetCourseReviewsAsync(int courseId);
        Task<CourseReview> AddReviewAsync(CourseReview review);
        Task<double> GetAverageRatingAsync(int courseId);
        Task<bool> HasUserReviewedAsync(string userId, int courseId);
    }
}