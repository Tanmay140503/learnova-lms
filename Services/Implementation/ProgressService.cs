using Learnova.Data;
using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnova.Services.Implementation
{
    public class ProgressService : IProgressService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnrollmentService _enrollmentService;

        public ProgressService(ApplicationDbContext context, IEnrollmentService enrollmentService)
        {
            _context = context;
            _enrollmentService = enrollmentService;
        }

        public async Task<LessonProgress?> GetLessonProgressAsync(string userId, int lessonId)
        {
            return await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);
        }

        public async Task<List<LessonProgress>> GetCourseProgressAsync(string userId, int courseId)
        {
            var lessonIds = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Select(l => l.Id)
                .ToListAsync();

            return await _context.LessonProgresses
                .Where(lp => lp.UserId == userId && lessonIds.Contains(lp.LessonId))
                .ToListAsync();
        }

        public async Task MarkLessonCompleteAsync(string userId, int lessonId)
        {
            var progress = await GetLessonProgressAsync(userId, lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow
                };
                _context.LessonProgresses.Add(progress);
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Update course enrollment status
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson != null)
            {
                await _enrollmentService.UpdateEnrollmentStatusAsync(userId, lesson.CourseId);
            }
        }

        public async Task<decimal> CalculateCourseCompletionAsync(string userId, int courseId)
        {
            var totalLessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .CountAsync();

            if (totalLessons == 0) return 0;

            var lessonIds = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Select(l => l.Id)
                .ToListAsync();

            var completedLessons = await _context.LessonProgresses
                .Where(lp => lp.UserId == userId && lessonIds.Contains(lp.LessonId) && lp.IsCompleted)
                .CountAsync();

            return (decimal)completedLessons / totalLessons * 100;
        }

        public async Task UpdateCourseProgressAsync(string userId, int courseId)
        {
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment != null)
            {
                enrollment.CompletionPercentage = await CalculateCourseCompletionAsync(userId, courseId);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddPointsToUserAsync(string userId, int points)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.TotalPoints += points;
                await _context.SaveChangesAsync();
                await UpdateUserBadgeAsync(userId);
            }
        }

        public async Task UpdateUserBadgeAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            var badge = user.TotalPoints switch
            {
                >= 120 => "Master",
                >= 100 => "Expert",
                >= 80 => "Specialist",
                >= 60 => "Achiever",
                >= 40 => "Explorer",
                >= 20 => "Newbie",
                _ => "Beginner"
            };

            user.BadgeLevel = badge;
            await _context.SaveChangesAsync();
        }
    }
}