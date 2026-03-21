using System.ComponentModel.DataAnnotations;

namespace Learnova.Models.Entities
{
    public class CourseReview
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? ReviewText { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}