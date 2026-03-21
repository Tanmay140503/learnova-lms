using Microsoft.AspNetCore.Identity;

namespace Learnova.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int TotalPoints { get; set; } = 0;
        public string? BadgeLevel { get; set; } = "Newbie";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<CourseEnrollment> Enrollments { get; set; }
        public ICollection<CourseReview> Reviews { get; set; }
        public ICollection<LessonProgress> LessonProgresses { get; set; }
        public ICollection<QuizAttempt> QuizAttempts { get; set; }
    }
}