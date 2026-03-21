using Microsoft.VisualBasic.FileIO;

namespace Learnova.Models.Entities
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public int OrderIndex { get; set; }

        // Foreign Key
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        // Navigation Properties
        public ICollection<QuizOption> Options { get; set; }
    }
}