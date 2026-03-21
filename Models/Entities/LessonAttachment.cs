namespace Learnova.Models.Entities
{
    public class LessonAttachment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AttachmentType { get; set; } // File, Link
        public string? FileUrl { get; set; }
        public string? ExternalLink { get; set; }

        // Foreign Key
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
    }
}