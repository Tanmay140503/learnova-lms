using Learnova.Models.Entities;

namespace Learnova.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<CourseEnrollment?> GetEnrollmentAsync(string userId, int courseId);
        Task<List<CourseEnrollment>> GetUserEnrollmentsAsync(string userId);
        Task<List<CourseEnrollment>> GetCourseEnrollmentsAsync(int courseId);
        Task<CourseEnrollment> EnrollUserAsync(string userId, int courseId);
        Task<bool> IsUserEnrolledAsync(string userId, int courseId);
        Task<bool> CanUserAccessCourseAsync(string userId, int courseId);
        Task UpdateEnrollmentStatusAsync(string userId, int courseId);
    }
}