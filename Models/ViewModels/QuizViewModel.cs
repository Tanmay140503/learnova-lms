using System.ComponentModel.DataAnnotations;

namespace Learnova.Models.ViewModels
{
    public class QuizViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }
        public int CourseId { get; set; }

        [Range(0, 100)]
        public int FirstAttemptPoints { get; set; } = 10;

        [Range(0, 100)]
        public int SecondAttemptPoints { get; set; } = 7;

        [Range(0, 100)]
        public int ThirdAttemptPoints { get; set; } = 5;

        [Range(0, 100)]
        public int FourthAttemptPoints { get; set; } = 3;

        public List<QuizQuestionViewModel>? Questions { get; set; }
    }

    public class QuizQuestionViewModel
    {
        public int Id { get; set; }

        [Required]
        public string QuestionText { get; set; }

        public int QuizId { get; set; }
        public List<QuizOptionViewModel>? Options { get; set; }
    }

    public class QuizOptionViewModel
    {
        public int Id { get; set; }

        [Required]
        public string OptionText { get; set; }

        public bool IsCorrect { get; set; }
        public int QuestionId { get; set; }
    }
}