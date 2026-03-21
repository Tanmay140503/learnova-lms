using Learnova.Data;
using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnova.Services.Implementation
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.ResponsibleUser)
                .Include(c => c.Lessons)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Course>> GetPublishedCoursesAsync(bool isAuthenticated)
        {
            var query = _context.Courses
                .Include(c => c.ResponsibleUser)
                .Include(c => c.Lessons)
                .Where(c => c.IsPublished);

            if (!isAuthenticated)
            {
                query = query.Where(c => c.Visibility == "Everyone");
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.ResponsibleUser)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Attachments)
                .Include(c => c.Quizzes)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qq => qq.Options)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<Course> UpdateCourseAsync(Course course)
        {
            course.UpdatedAt = DateTime.UtcNow;

            // Calculate totals
            var lessons = await _context.Lessons.Where(l => l.CourseId == course.Id).ToListAsync();
            course.TotalLessons = lessons.Count;
            course.TotalDurationMinutes = lessons.Sum(l => l.DurationMinutes ?? 0);

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TogglePublishAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            course.IsPublished = !course.IsPublished;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task IncrementViewCountAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.ViewsCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Course>> SearchCoursesAsync(string searchTerm)
        {
            return await _context.Courses
                .Include(c => c.Lessons)
                .Where(c => c.Title.Contains(searchTerm) || c.Tags.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<List<Course>> GetCoursesByInstructorAsync(string userId)
        {
            return await _context.Courses
                .Include(c => c.Lessons)
                .Where(c => c.ResponsibleUserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}