namespace Learnova.Models.Entities
{
    public class QuizOption
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; } = false;

        // Foreign Key
        public int QuestionId { get; set; }
        public QuizQuestion Question { get; set; }
    }
}