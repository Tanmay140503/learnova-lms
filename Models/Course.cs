using System.ComponentModel.DataAnnotations;

namespace Learnova.Models.Entities
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public string? Tags { get; set; }
        public string? CourseImageUrl { get; set; }
        public string? Website { get; set; }

        // Publishing & Visibility
        public bool IsPublished { get; set; } = false;
        public string Visibility { get; set; } = "Everyone"; // Everyone, SignedIn
        public string AccessRule { get; set; } = "Open"; // Open, OnInvitation, OnPayment
        public decimal? Price { get; set; }

        // Stats
        public int ViewsCount { get; set; } = 0;
        public int TotalLessons { get; set; } = 0;
        public int TotalDurationMinutes { get; set; } = 0;

        // Admin/Responsible
        public string? ResponsibleUserId { get; set; }
        public ApplicationUser? ResponsibleUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<Lesson> Lessons { get; set; }
        public ICollection<Quiz> Quizzes { get; set; }
        public ICollection<CourseEnrollment> Enrollments { get; set; }
        public ICollection<CourseReview> Reviews { get; set; }
    }
}