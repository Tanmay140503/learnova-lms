namespace Learnova.Models.Entities
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }
        public int TimeSpentMinutes { get; set; } = 0;
    }
}