using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnova.Models.Entities
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string LessonType { get; set; } = "Video"; // Video, Document, Image, Quiz

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

        [ForeignKey("ResponsibleUserId")]
        public ApplicationUser? ResponsibleUser { get; set; }

        // Foreign Key
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //  Navigation Properties 
        public ICollection<LessonAttachment> LessonAttachments { get; set; } = new List<LessonAttachment>();
        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}
