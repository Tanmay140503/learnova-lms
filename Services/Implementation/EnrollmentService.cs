using Learnova.Data;
using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnova.Services.Implementation
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CourseEnrollment?> GetEnrollmentAsync(string userId, int courseId)
        {
            return await _context.CourseEnrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        }

        public async Task<List<CourseEnrollment>> GetUserEnrollmentsAsync(string userId)
        {
            return await _context.CourseEnrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledDate)
                .ToListAsync();
        }

        public async Task<List<CourseEnrollment>> GetCourseEnrollmentsAsync(int courseId)
        {
            return await _context.CourseEnrollments
                .Include(e => e.User)
                .Where(e => e.CourseId == courseId)
                .OrderByDescending(e => e.EnrolledDate)
                .ToListAsync();
        }

        public async Task<CourseEnrollment> EnrollUserAsync(string userId, int courseId)
        {
            var existingEnrollment = await GetEnrollmentAsync(userId, courseId);
            if (existingEnrollment != null)
                return existingEnrollment;

            var enrollment = new CourseEnrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledDate = DateTime.UtcNow,
                Status = "Yet to Start"
            };

            _context.CourseEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<bool> IsUserEnrolledAsync(string userId, int courseId)
        {
            return await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        }

        public async Task<bool> CanUserAccessCourseAsync(string userId, int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            // If Open access, anyone enrolled can access
            if (course.AccessRule == "Open")
                return await IsUserEnrolledAsync(userId, courseId);

            // If OnInvitation, check if enrolled
            if (course.AccessRule == "OnInvitation")
                return await IsUserEnrolledAsync(userId, courseId);

            // If OnPayment, check if paid
            if (course.AccessRule == "OnPayment")
            {
                var enrollment = await GetEnrollmentAsync(userId, courseId);
                return enrollment != null && enrollment.IsPaid;
            }

            return false;
        }

        public async Task UpdateEnrollmentStatusAsync(string userId, int courseId)
        {
            var enrollment = await GetEnrollmentAsync(userId, courseId);
            if (enrollment == null) return;

            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null || course.Lessons.Count == 0) return;

            var completedLessons = await _context.LessonProgresses
                .Where(lp => lp.UserId == userId &&
                       course.Lessons.Select(l => l.Id).Contains(lp.LessonId) &&
                       lp.IsCompleted)
                .CountAsync();

            enrollment.CompletionPercentage = (decimal)completedLessons / course.Lessons.Count * 100;

            if (completedLessons == 0)
            {
                enrollment.Status = "Yet to Start";
                enrollment.StartDate = null;
            }
            else if (completedLessons < course.Lessons.Count)
            {
                enrollment.Status = "In Progress";
                if (enrollment.StartDate == null)
                    enrollment.StartDate = DateTime.UtcNow;
            }
            else
            {
                enrollment.Status = "Completed";
                if (enrollment.CompletedDate == null)
                    enrollment.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
        public async Task MarkCourseCompletedAsync(string userId, int courseId)
        {
            var enrollment = await GetEnrollmentAsync(userId, courseId);
            if (enrollment == null) return;

            enrollment.Status = "Completed";
            enrollment.CompletionPercentage = 100;
            enrollment.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
