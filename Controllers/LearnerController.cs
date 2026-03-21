using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Learnova.Controllers
{
    public class LearnerController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;
        private readonly IQuizService _quizService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IProgressService _progressService;
        private readonly IReviewService _reviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LearnerController(
            ICourseService courseService,
            ILessonService lessonService,
            IQuizService quizService,
            IEnrollmentService enrollmentService,
            IProgressService progressService,
            IReviewService reviewService,
            UserManager<ApplicationUser> userManager)
        {
            _courseService = courseService;
            _lessonService = lessonService;
            _quizService = quizService;
            _enrollmentService = enrollmentService;
            _progressService = progressService;
            _reviewService = reviewService;
            _userManager = userManager;
        }

        // ============ BROWSE COURSES ============

        public async Task<IActionResult> Courses(string search = "")
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var courses = await _courseService.GetPublishedCoursesAsync(isAuthenticated);

            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(c =>
                    c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (c.Tags != null && c.Tags.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.Search = search;

            if (isAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.UserId = currentUser?.Id;
            }

            return View(courses);
        }

        // ============ MY COURSES ============

        [Authorize]
        public async Task<IActionResult> MyCourses(string search = "")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var enrollments = await _enrollmentService.GetUserEnrollmentsAsync(currentUser.Id);

            if (!string.IsNullOrEmpty(search))
            {
                enrollments = enrollments.Where(e =>
                    e.Course.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (e.Course.Tags != null && e.Course.Tags.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.Search = search;
            ViewBag.User = currentUser;

            return View(enrollments);
        }

        // ============ COURSE DETAIL ============

        public async Task<IActionResult> CourseDetail(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            // Check visibility
            if (!course.IsPublished)
            {
                if (!User.Identity.IsAuthenticated ||
                    (!User.IsInRole("Admin") && !User.IsInRole("Instructor")))
                {
                    return NotFound();
                }
            }

            // Increment view count
            await _courseService.IncrementViewCountAsync(id);

            string? userId = null;
            CourseEnrollment? enrollment = null;
            List<LessonProgress>? progresses = null;

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                userId = currentUser?.Id;

                if (userId != null)
                {
                    enrollment = await _enrollmentService.GetEnrollmentAsync(userId, id);
                    if (enrollment != null)
                    {
                        progresses = await _progressService.GetCourseProgressAsync(userId, id);
                    }
                }
            }

            // Get reviews
            var reviews = await _reviewService.GetCourseReviewsAsync(id);
            var avgRating = await _reviewService.GetAverageRatingAsync(id);

            ViewBag.UserId = userId;
            ViewBag.Enrollment = enrollment;
            ViewBag.Progresses = progresses;
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = avgRating;

            return View(course);
        }

        // ============ JOIN COURSE ============

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> JoinCourse(int courseId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
                return Json(new { success = false, message = "Course not found" });

            // Check if already enrolled
            var existingEnrollment = await _enrollmentService.GetEnrollmentAsync(currentUser.Id, courseId);
            if (existingEnrollment != null)
                return Json(new { success = true, message = "Already enrolled" });

            // Check access rules
            if (course.AccessRule == "OnPayment" && course.Price > 0)
            {
                return Json(new
                {
                    success = false,
                    requiresPayment = true,
                    price = course.Price,
                    message = "This is a paid course"
                });
            }

            if (course.AccessRule == "OnInvitation")
            {
                return Json(new
                {
                    success = false,
                    message = "This course is invitation-only. Please contact the instructor."
                });
            }

            // Enroll user
            await _enrollmentService.EnrollUserAsync(currentUser.Id, courseId);

            return Json(new { success = true, message = "Successfully enrolled!" });
        }

        // ============ LESSON PLAYER ============

        [Authorize]
        public async Task<IActionResult> Player(int lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            if (lesson == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            // Check enrollment
            var enrollment = await _enrollmentService.GetEnrollmentAsync(currentUser.Id, lesson.CourseId);
            if (enrollment == null)
            {
                TempData["Error"] = "Please enroll in this course first";
                return RedirectToAction("CourseDetail", new { id = lesson.CourseId });
            }

            // Update start date if not set
            if (enrollment.StartDate == null)
            {
                enrollment.StartDate = DateTime.UtcNow;
                enrollment.Status = "In Progress";
                await _enrollmentService.UpdateEnrollmentStatusAsync(currentUser.Id, lesson.CourseId);
            }

            var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
            var lessons = await _lessonService.GetLessonsByCourseIdAsync(lesson.CourseId);
            var progresses = await _progressService.GetCourseProgressAsync(currentUser.Id, lesson.CourseId);

            ViewBag.Course = course;
            ViewBag.Lessons = lessons;
            ViewBag.Progresses = progresses;
            ViewBag.Enrollment = enrollment;
            ViewBag.CurrentUser = currentUser;

            return View(lesson);
        }

        // ============ COMPLETE LESSON ============

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null)
                return Json(new { success = false, message = "Lesson not found" });

            await _progressService.MarkLessonCompleteAsync(currentUser.Id, lessonId);

            var completion = await _progressService.CalculateCourseCompletionAsync(currentUser.Id, lesson.CourseId);

            return Json(new
            {
                success = true,
                completionPercentage = completion,
                message = "Lesson completed!"
            });
        }

        // ============ REVIEWS ============

        public async Task<IActionResult> GetReviews(int courseId)
        {
            var reviews = await _reviewService.GetCourseReviewsAsync(courseId);
            var averageRating = await _reviewService.GetAverageRatingAsync(courseId);

            var reviewsData = reviews.Select(r => new {
                id = r.Id,
                rating = r.Rating,
                reviewText = r.ReviewText,
                createdAt = r.CreatedAt.ToString("MMM dd, yyyy"),
                user = new
                {
                    firstName = r.User.FirstName,
                    lastName = r.User.LastName
                }
            });

            return Json(new { reviews = reviewsData, averageRating });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddReview(int courseId, int rating, string reviewText)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Check if user is enrolled
            var enrollment = await _enrollmentService.GetEnrollmentAsync(currentUser.Id, courseId);
            if (enrollment == null)
                return Json(new { success = false, message = "You must be enrolled to review this course" });

            // Check if already reviewed
            var hasReviewed = await _reviewService.HasUserReviewedAsync(currentUser.Id, courseId);
            if (hasReviewed)
                return Json(new { success = false, message = "You have already reviewed this course" });

            if (rating < 1 || rating > 5)
                return Json(new { success = false, message = "Rating must be between 1 and 5" });

            var review = new CourseReview
            {
                UserId = currentUser.Id,
                CourseId = courseId,
                Rating = rating,
                ReviewText = reviewText,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewService.AddReviewAsync(review);

            return Json(new { success = true, message = "Review added successfully!" });
        }

        // ============ QUIZ ============

        [Authorize]
        public async Task<IActionResult> TakeQuiz(int quizId)
        {
            var quiz = await _quizService.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var attempts = await _quizService.GetUserAttemptsAsync(currentUser.Id, quizId);

            ViewBag.AttemptCount = attempts.Count;
            ViewBag.BestScore = attempts.Any() ? attempts.Max(a => a.Score) : 0;

            return View(quiz);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionModel submission)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var quiz = await _quizService.GetQuizWithQuestionsAsync(submission.QuizId);

            if (quiz == null)
                return Json(new { success = false, message = "Quiz not found" });

            // Calculate score
            var answers = submission.Answers ?? new Dictionary<int, int>();
            var score = await _quizService.CalculateScoreAsync(submission.QuizId, answers);
            var isPassed = score >= 60;

            // Get attempt number
            var attemptCount = await _quizService.GetAttemptCountAsync(currentUser.Id, submission.QuizId);
            var attemptNumber = attemptCount + 1;

            // Calculate points
            var pointsEarned = 0;
            if (isPassed)
            {
                pointsEarned = await _quizService.CalculatePointsAsync(submission.QuizId, attemptNumber);
            }

            // Record attempt
            var attempt = new QuizAttempt
            {
                UserId = currentUser.Id,
                QuizId = submission.QuizId,
                AttemptNumber = attemptNumber,
                Score = score,
                PointsEarned = pointsEarned,
                IsPassed = isPassed,
                AnswersJson = JsonSerializer.Serialize(answers),
                AttemptDate = DateTime.UtcNow
            };

            await _quizService.RecordAttemptAsync(attempt);

            // Add points to user if passed
            if (isPassed)
            {
                await _progressService.AddPointsToUserAsync(currentUser.Id, pointsEarned);
            }

            // Get updated user info
            var updatedUser = await _userManager.FindByIdAsync(currentUser.Id);

            return Json(new
            {
                success = true,
                score,
                isPassed,
                pointsEarned,
                attemptNumber,
                totalPoints = updatedUser.TotalPoints,
                badgeLevel = updatedUser.BadgeLevel
            });
        }
    }

    // Model for quiz submission
    public class QuizSubmissionModel
    {
        public int QuizId { get; set; }
        public Dictionary<int, int> Answers { get; set; }
    }
}