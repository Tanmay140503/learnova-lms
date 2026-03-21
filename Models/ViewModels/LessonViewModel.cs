using System.ComponentModel.DataAnnotations;

namespace Learnova.Models.ViewModels
{
    public class LessonViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string LessonType { get; set; }

        // Video fields
        public string? VideoUrl { get; set; }
        public int? DurationMinutes { get; set; }

        // Document fields
        public IFormFile? DocumentFile { get; set; }
        public string? DocumentUrl { get; set; }

        // Image fields
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }

        public bool AllowDownload { get; set; }

        public string? ResponsibleUserId { get; set; }
        public int CourseId { get; set; }

        public List<AttachmentViewModel>? Attachments { get; set; }
    }

    public class AttachmentViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AttachmentType { get; set; }
        public IFormFile? File { get; set; }
        public string? FileUrl { get; set; }
        public string? ExternalLink { get; set; }
    }
}