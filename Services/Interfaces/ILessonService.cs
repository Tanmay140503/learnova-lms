using Learnova.Models.Entities;

namespace Learnova.Services.Interfaces
{
    public interface ILessonService
    {
        // Lesson CRUD
        Task<List<Lesson>> GetAllLessonsAsync();
        Task<List<Lesson>> GetLessonsByCourseIdAsync(int courseId);
        Task<Lesson?> GetLessonByIdAsync(int id);
        Task<Lesson> CreateLessonAsync(Lesson lesson);
        Task<Lesson> UpdateLessonAsync(Lesson lesson);
        Task<bool> DeleteLessonAsync(int id);

        // Lesson Ordering
        Task<bool> ReorderLessonsAsync(int courseId, List<int> lessonIds);
        Task<Lesson?> GetNextLessonAsync(int currentLessonId);
        Task<Lesson?> GetPreviousLessonAsync(int currentLessonId);
        Task<Lesson?> GetFirstLessonAsync(int courseId);

        // Attachments
        Task<List<LessonAttachment>> GetAttachmentsByLessonIdAsync(int lessonId);
        Task<LessonAttachment?> GetAttachmentByIdAsync(int id);
        Task<LessonAttachment> AddAttachmentAsync(LessonAttachment attachment);
        Task<bool> DeleteAttachmentAsync(int id);

        // Stats
        Task<int> GetLessonCountByCourseAsync(int courseId);
        Task<int> GetTotalDurationByCourseAsync(int courseId);
    }
}