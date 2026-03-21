using Learnova.Models.Entities;

namespace Learnova.Services.Interfaces
{
    public interface IQuizService
    {
        // Quiz CRUD
        Task<List<Quiz>> GetAllQuizzesAsync();
        Task<List<Quiz>> GetQuizzesByCourseIdAsync(int courseId);
        Task<Quiz?> GetQuizByIdAsync(int id);
        Task<Quiz?> GetQuizWithQuestionsAsync(int id);
        Task<Quiz> CreateQuizAsync(Quiz quiz);
        Task<Quiz> UpdateQuizAsync(Quiz quiz);
        Task<bool> DeleteQuizAsync(int id);

        // Questions CRUD
        Task<List<QuizQuestion>> GetQuestionsByQuizIdAsync(int quizId);
        Task<QuizQuestion?> GetQuestionByIdAsync(int id);
        Task<QuizQuestion> AddQuestionAsync(QuizQuestion question);
        Task<QuizQuestion> UpdateQuestionAsync(QuizQuestion question);
        Task<bool> DeleteQuestionAsync(int id);
        Task<bool> ReorderQuestionsAsync(int quizId, List<int> questionIds);

        // Options CRUD
        Task<List<QuizOption>> GetOptionsByQuestionIdAsync(int questionId);
        Task<QuizOption?> GetOptionByIdAsync(int id);
        Task<QuizOption> AddOptionAsync(QuizOption option);
        Task<QuizOption> UpdateOptionAsync(QuizOption option);
        Task<bool> DeleteOptionAsync(int id);
        Task<bool> SetCorrectOptionAsync(int optionId);

        // Quiz Attempts
        Task<QuizAttempt> RecordAttemptAsync(QuizAttempt attempt);
        Task<List<QuizAttempt>> GetUserAttemptsAsync(string userId, int quizId);
        Task<List<QuizAttempt>> GetAllAttemptsForQuizAsync(int quizId);
        Task<int> GetAttemptCountAsync(string userId, int quizId);
        Task<QuizAttempt?> GetBestAttemptAsync(string userId, int quizId);

        // Scoring
        Task<int> CalculatePointsAsync(int quizId, int attemptNumber);
        Task<int> CalculateScoreAsync(int quizId, Dictionary<int, int> answers);
        Task<bool> CheckAnswerAsync(int questionId, int selectedOptionId);

        // Stats
        Task<double> GetAverageScoreAsync(int quizId);
        Task<int> GetTotalAttemptsAsync(int quizId);
        Task<double> GetPassRateAsync(int quizId);
    }
}