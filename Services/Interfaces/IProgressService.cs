using Learnova.Models.Entities;

namespace Learnova.Services.Interfaces
{
    public interface IProgressService
    {
        Task<LessonProgress?> GetLessonProgressAsync(string userId, int lessonId);
        Task<List<LessonProgress>> GetCourseProgressAsync(string userId, int courseId);
        Task MarkLessonCompleteAsync(string userId, int lessonId);
        Task<decimal> CalculateCourseCompletionAsync(string userId, int courseId);
        Task UpdateCourseProgressAsync(string userId, int courseId);
        Task AddPointsToUserAsync(string userId, int points);
        Task UpdateUserBadgeAsync(string userId);
    }
}