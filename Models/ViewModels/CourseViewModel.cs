using System.ComponentModel.DataAnnotations;

namespace Learnova.Models.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course title is required")]
        [StringLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public string? Tags { get; set; }
        public IFormFile? CourseImage { get; set; }
        public string? CourseImageUrl { get; set; }
        public string? Website { get; set; }

        public bool IsPublished { get; set; }
        public string Visibility { get; set; } = "Everyone";
        public string AccessRule { get; set; } = "Open";

        [Range(0, 999999)]
        public decimal? Price { get; set; }

        public string? ResponsibleUserId { get; set; }

        public int ViewsCount { get; set; }
        public int TotalLessons { get; set; }
        public int TotalDurationMinutes { get; set; }
    }
}