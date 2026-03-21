using Learnova.Data;
using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Learnova.Services.Implementation
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============ QUIZ CRUD ============

        public async Task<List<Quiz>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesByCourseIdAsync(int courseId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .Where(q => q.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizByIdAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Quiz?> GetQuizWithQuestionsAsync(int id)
        {
            return await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions.OrderBy(qq => qq.OrderIndex))
                    .ThenInclude(qq => qq.Options)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Quiz> CreateQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return quiz;
        }

        public async Task<Quiz> UpdateQuizAsync(Quiz quiz)
        {
            var existingQuiz = await _context.Quizzes.FindAsync(quiz.Id);
            if (existingQuiz == null)
                throw new Exception("Quiz not found");

            existingQuiz.Title = quiz.Title;
            existingQuiz.Description = quiz.Description;
            existingQuiz.FirstAttemptPoints = quiz.FirstAttemptPoints;
            existingQuiz.SecondAttemptPoints = quiz.SecondAttemptPoints;
            existingQuiz.ThirdAttemptPoints = quiz.ThirdAttemptPoints;
            existingQuiz.FourthAttemptPoints = quiz.FourthAttemptPoints;

            await _context.SaveChangesAsync();
            return existingQuiz;
        }

        public async Task<bool> DeleteQuizAsync(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .Include(q => q.Attempts)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return false;

            // Delete all related data
            foreach (var question in quiz.Questions)
            {
                _context.QuizOptions.RemoveRange(question.Options);
            }
            _context.QuizQuestions.RemoveRange(quiz.Questions);
            _context.QuizAttempts.RemoveRange(quiz.Attempts);
            _context.Quizzes.Remove(quiz);

            await _context.SaveChangesAsync();
            return true;
        }

        // ============ QUESTIONS CRUD ============

        public async Task<List<QuizQuestion>> GetQuestionsByQuizIdAsync(int quizId)
        {
            return await _context.QuizQuestions
                .Include(q => q.Options)
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();
        }

        public async Task<QuizQuestion?> GetQuestionByIdAsync(int id)
        {
            return await _context.QuizQuestions
                .Include(q => q.Options)
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<QuizQuestion> AddQuestionAsync(QuizQuestion question)
        {
            // Get max order index
            var maxOrder = await _context.QuizQuestions
                .Where(q => q.QuizId == question.QuizId)
                .MaxAsync(q => (int?)q.OrderIndex) ?? 0;

            question.OrderIndex = maxOrder + 1;

            _context.QuizQuestions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<QuizQuestion> UpdateQuestionAsync(QuizQuestion question)
        {
            var existingQuestion = await _context.QuizQuestions.FindAsync(question.Id);
            if (existingQuestion == null)
                throw new Exception("Question not found");

            existingQuestion.QuestionText = question.QuestionText;
            existingQuestion.OrderIndex = question.OrderIndex;

            await _context.SaveChangesAsync();
            return existingQuestion;
        }

        public async Task<bool> DeleteQuestionAsync(int id)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null) return false;

            var quizId = question.QuizId;

            // Delete options first
            _context.QuizOptions.RemoveRange(question.Options);
            _context.QuizQuestions.Remove(question);

            await _context.SaveChangesAsync();

            // Reorder remaining questions
            await ReorderQuestionsAfterDeleteAsync(quizId);

            return true;
        }

        public async Task<bool> ReorderQuestionsAsync(int quizId, List<int> questionIds)
        {
            var questions = await _context.QuizQuestions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            for (int i = 0; i < questionIds.Count; i++)
            {
                var question = questions.FirstOrDefault(q => q.Id == questionIds[i]);
                if (question != null)
                {
                    question.OrderIndex = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ============ OPTIONS CRUD ============

        public async Task<List<QuizOption>> GetOptionsByQuestionIdAsync(int questionId)
        {
            return await _context.QuizOptions
                .Where(o => o.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task<QuizOption?> GetOptionByIdAsync(int id)
        {
            return await _context.QuizOptions
                .Include(o => o.Question)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<QuizOption> AddOptionAsync(QuizOption option)
        {
            _context.QuizOptions.Add(option);
            await _context.SaveChangesAsync();
            return option;
        }

        public async Task<QuizOption> UpdateOptionAsync(QuizOption option)
        {
            var existingOption = await _context.QuizOptions.FindAsync(option.Id);
            if (existingOption == null)
                throw new Exception("Option not found");

            existingOption.OptionText = option.OptionText;
            existingOption.IsCorrect = option.IsCorrect;

            await _context.SaveChangesAsync();
            return existingOption;
        }

        public async Task<bool> DeleteOptionAsync(int id)
        {
            var option = await _context.QuizOptions.FindAsync(id);
            if (option == null) return false;

            _context.QuizOptions.Remove(option);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetCorrectOptionAsync(int optionId)
        {
            var option = await _context.QuizOptions
                .Include(o => o.Question)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(o => o.Id == optionId);

            if (option == null) return false;

            // Set all options of this question to false
            foreach (var opt in option.Question.Options)
            {
                opt.IsCorrect = false;
            }

            // Set selected option to true
            option.IsCorrect = true;

            await _context.SaveChangesAsync();
            return true;
        }

        // ============ QUIZ ATTEMPTS ============

        public async Task<QuizAttempt> RecordAttemptAsync(QuizAttempt attempt)
        {
            attempt.AttemptDate = DateTime.UtcNow;
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<List<QuizAttempt>> GetUserAttemptsAsync(string userId, int quizId)
        {
            return await _context.QuizAttempts
                .Include(a => a.Quiz)
                .Where(a => a.UserId == userId && a.QuizId == quizId)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();
        }

        public async Task<List<QuizAttempt>> GetAllAttemptsForQuizAsync(int quizId)
        {
            return await _context.QuizAttempts
                .Include(a => a.User)
                .Where(a => a.QuizId == quizId)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();
        }

        public async Task<int> GetAttemptCountAsync(string userId, int quizId)
        {
            return await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.QuizId == quizId)
                .CountAsync();
        }

        public async Task<QuizAttempt?> GetBestAttemptAsync(string userId, int quizId)
        {
            return await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.QuizId == quizId)
                .OrderByDescending(a => a.Score)
                .FirstOrDefaultAsync();
        }

        // ============ SCORING ============

        public async Task<int> CalculatePointsAsync(int quizId, int attemptNumber)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null) return 0;

            return attemptNumber switch
            {
                1 => quiz.FirstAttemptPoints,
                2 => quiz.SecondAttemptPoints,
                3 => quiz.ThirdAttemptPoints,
                _ => quiz.FourthAttemptPoints
            };
        }

        public async Task<int> CalculateScoreAsync(int quizId, Dictionary<int, int> answers)
        {
            var quiz = await GetQuizWithQuestionsAsync(quizId);
            if (quiz == null || quiz.Questions == null || !quiz.Questions.Any())
                return 0;

            int correctAnswers = 0;
            int totalQuestions = quiz.Questions.Count;

            foreach (var question in quiz.Questions)
            {
                if (answers.ContainsKey(question.Id))
                {
                    var selectedOptionId = answers[question.Id];
                    var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);

                    if (correctOption != null && correctOption.Id == selectedOptionId)
                    {
                        correctAnswers++;
                    }
                }
            }

            // Return percentage score
            return totalQuestions > 0 ? (int)Math.Round((double)correctAnswers / totalQuestions * 100) : 0;
        }

        public async Task<bool> CheckAnswerAsync(int questionId, int selectedOptionId)
        {
            var option = await _context.QuizOptions
                .FirstOrDefaultAsync(o => o.Id == selectedOptionId && o.QuestionId == questionId);

            return option?.IsCorrect ?? false;
        }

        // ============ STATS ============

        public async Task<double> GetAverageScoreAsync(int quizId)
        {
            var attempts = await _context.QuizAttempts
                .Where(a => a.QuizId == quizId)
                .ToListAsync();

            if (!attempts.Any()) return 0;

            return attempts.Average(a => a.Score);
        }

        public async Task<int> GetTotalAttemptsAsync(int quizId)
        {
            return await _context.QuizAttempts
                .Where(a => a.QuizId == quizId)
                .CountAsync();
        }

        public async Task<double> GetPassRateAsync(int quizId)
        {
            var attempts = await _context.QuizAttempts
                .Where(a => a.QuizId == quizId)
                .ToListAsync();

            if (!attempts.Any()) return 0;

            var passedAttempts = attempts.Count(a => a.IsPassed);
            return (double)passedAttempts / attempts.Count * 100;
        }

        // ============ PRIVATE HELPERS ============

        private async Task ReorderQuestionsAfterDeleteAsync(int quizId)
        {
            var questions = await _context.QuizQuestions
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();

            for (int i = 0; i < questions.Count; i++)
            {
                questions[i].OrderIndex = i + 1;
            }

            await _context.SaveChangesAsync();
        }
    }
}
