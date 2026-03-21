using Learnova.Data;
using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnova.Services.Implementation
{
    public class LessonService : ILessonService
    {
        private readonly ApplicationDbContext _context;

        public LessonService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============ LESSON CRUD ============

        public async Task<List<Lesson>> GetAllLessonsAsync()
        {
            return await _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.ResponsibleUser)
                .Include(l => l.Attachments)
                .OrderBy(l => l.CourseId)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();
        }

        public async Task<List<Lesson>> GetLessonsByCourseIdAsync(int courseId)
        {
            return await _context.Lessons
                .Include(l => l.ResponsibleUser)
                .Include(l => l.Attachments)
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();
        }

        public async Task<Lesson?> GetLessonByIdAsync(int id)
        {
            return await _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.ResponsibleUser)
                .Include(l => l.Attachments)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Lesson> CreateLessonAsync(Lesson lesson)
        {
            // Get the max order index for this course
            var maxOrder = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId)
                .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

            lesson.OrderIndex = maxOrder + 1;
            lesson.CreatedAt = DateTime.UtcNow;

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            // Update course totals
            await UpdateCourseTotalsAsync(lesson.CourseId);

            return lesson;
        }

        public async Task<Lesson> UpdateLessonAsync(Lesson lesson)
        {
            var existingLesson = await _context.Lessons.FindAsync(lesson.Id);
            if (existingLesson == null)
                throw new Exception("Lesson not found");

            // Update properties
            existingLesson.Title = lesson.Title;
            existingLesson.Description = lesson.Description;
            existingLesson.LessonType = lesson.LessonType;
            existingLesson.VideoUrl = lesson.VideoUrl;
            existingLesson.DurationMinutes = lesson.DurationMinutes;
            existingLesson.DocumentUrl = lesson.DocumentUrl;
            existingLesson.ImageUrl = lesson.ImageUrl;
            existingLesson.AllowDownload = lesson.AllowDownload;
            existingLesson.ResponsibleUserId = lesson.ResponsibleUserId;

            await _context.SaveChangesAsync();

            // Update course totals
            await UpdateCourseTotalsAsync(existingLesson.CourseId);

            return existingLesson;
        }

        public async Task<bool> DeleteLessonAsync(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Attachments)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                return false;

            var courseId = lesson.CourseId;

            // Delete attachments first
            if (lesson.Attachments != null && lesson.Attachments.Any())
            {
                _context.LessonAttachments.RemoveRange(lesson.Attachments);
            }

            // Delete lesson progress records
            var progresses = await _context.LessonProgresses
                .Where(lp => lp.LessonId == id)
                .ToListAsync();
            _context.LessonProgresses.RemoveRange(progresses);

            // Delete lesson
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            // Update course totals
            await UpdateCourseTotalsAsync(courseId);

            // Reorder remaining lessons
            await ReorderAfterDeleteAsync(courseId);

            return true;
        }

        // ============ LESSON ORDERING ============

        public async Task<bool> ReorderLessonsAsync(int courseId, List<int> lessonIds)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .ToListAsync();

            for (int i = 0; i < lessonIds.Count; i++)
            {
                var lesson = lessons.FirstOrDefault(l => l.Id == lessonIds[i]);
                if (lesson != null)
                {
                    lesson.OrderIndex = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Lesson?> GetNextLessonAsync(int currentLessonId)
        {
            var currentLesson = await _context.Lessons.FindAsync(currentLessonId);
            if (currentLesson == null) return null;

            return await _context.Lessons
                .Where(l => l.CourseId == currentLesson.CourseId && l.OrderIndex > currentLesson.OrderIndex)
                .OrderBy(l => l.OrderIndex)
                .FirstOrDefaultAsync();
        }

        public async Task<Lesson?> GetPreviousLessonAsync(int currentLessonId)
        {
            var currentLesson = await _context.Lessons.FindAsync(currentLessonId);
            if (currentLesson == null) return null;

            return await _context.Lessons
                .Where(l => l.CourseId == currentLesson.CourseId && l.OrderIndex < currentLesson.OrderIndex)
                .OrderByDescending(l => l.OrderIndex)
                .FirstOrDefaultAsync();
        }

        public async Task<Lesson?> GetFirstLessonAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.OrderIndex)
                .FirstOrDefaultAsync();
        }

        // ============ ATTACHMENTS ============

        public async Task<List<LessonAttachment>> GetAttachmentsByLessonIdAsync(int lessonId)
        {
            return await _context.LessonAttachments
                .Where(a => a.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<LessonAttachment?> GetAttachmentByIdAsync(int id)
        {
            return await _context.LessonAttachments.FindAsync(id);
        }

        public async Task<LessonAttachment> AddAttachmentAsync(LessonAttachment attachment)
        {
            _context.LessonAttachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<bool> DeleteAttachmentAsync(int id)
        {
            var attachment = await _context.LessonAttachments.FindAsync(id);
            if (attachment == null) return false;

            _context.LessonAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============ STATS ============

        public async Task<int> GetLessonCountByCourseAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .CountAsync();
        }

        public async Task<int> GetTotalDurationByCourseAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .SumAsync(l => l.DurationMinutes ?? 0);
        }

        // ============ PRIVATE HELPERS ============

        private async Task UpdateCourseTotalsAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                var lessons = await _context.Lessons
                    .Where(l => l.CourseId == courseId)
                    .ToListAsync();

                course.TotalLessons = lessons.Count;
                course.TotalDurationMinutes = lessons.Sum(l => l.DurationMinutes ?? 0);
                course.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        private async Task ReorderAfterDeleteAsync(int courseId)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();

            for (int i = 0; i < lessons.Count; i++)
            {
                lessons[i].OrderIndex = i + 1;
            }

            await _context.SaveChangesAsync();
        }
    }
}