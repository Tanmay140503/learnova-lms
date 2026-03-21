namespace Learnova.Models.Entities
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public int AttemptNumber { get; set; }
        public int Score { get; set; }
        public int PointsEarned { get; set; }
        public bool IsPassed { get; set; } = false;

        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

        // Store answers
        public string? AnswersJson { get; set; } // JSON string of answers
    }
}