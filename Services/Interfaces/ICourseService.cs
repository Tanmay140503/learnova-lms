using Learnova.Models.Entities;

namespace Learnova.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<List<Course>> GetPublishedCoursesAsync(bool isAuthenticated);
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course> CreateCourseAsync(Course course);
        Task<Course> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> TogglePublishAsync(int id);
        Task IncrementViewCountAsync(int id);
        Task<List<Course>> SearchCoursesAsync(string searchTerm);
        Task<List<Course>> GetCoursesByInstructorAsync(string userId);
    }
}
