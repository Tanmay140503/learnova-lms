namespace Learnova.Models.Entities
{
    public class CourseEnrollment
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public int TimeSpentMinutes { get; set; } = 0;
        public decimal CompletionPercentage { get; set; } = 0;

        public string Status { get; set; } = "Yet to Start"; // Yet to Start, In Progress, Completed

        public bool IsPaid { get; set; } = false;
    }
}