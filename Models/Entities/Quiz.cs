using System.ComponentModel.DataAnnotations;

namespace Learnova.Models.Entities
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        // Rewards (points based on attempt)
        public int FirstAttemptPoints { get; set; } = 10;
        public int SecondAttemptPoints { get; set; } = 7;
        public int ThirdAttemptPoints { get; set; } = 5;
        public int FourthAttemptPoints { get; set; } = 3;

        // Foreign Key
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // Navigation Properties
        public ICollection<QuizQuestion> Questions { get; set; }
        public ICollection<QuizAttempt> Attempts { get; set; }
    }
}