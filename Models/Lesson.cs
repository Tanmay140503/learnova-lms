using System.ComponentModel.DataAnnotations;

namespace Learnova.Models.Entities
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string LessonType { get; set; } // Video, Document, Image, Quiz

        // Type-specific fields
        public string? VideoUrl { get; set; }
        public int? DurationMinutes { get; set; }
        public string? DocumentUrl { get; set; }
        public string? ImageUrl { get; set; }
        public bool AllowDownload { get; set; } = false;

        // Order
        public int OrderIndex { get; set; }

        // Responsible
        public string? ResponsibleUserId { get; set; }
        public ApplicationUser? ResponsibleUser { get; set; }

        // Foreign Key
        public int CourseId { get; set; }
        public Course Course { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<LessonAttachment> Attachments { get; set; }
        public ICollection<LessonProgress> LessonProgresses { get; set; }
    }
}