using Learnova.Data;
using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Learnova.Controllers
{
    [Authorize]
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
            ICourseService courseService, ILessonService lessonService, IQuizService quizService,
            IEnrollmentService enrollmentService, IProgressService progressService, IReviewService reviewService,
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

        // Helper: Badge Calculation
        private string CalculateBadge(int points)
        {
            return points switch
            {
                >= 120 => "Master",
                >= 100 => "Expert",
                >= 80 => "Specialist",
                >= 60 => "Achiever",
                >= 40 => "Explorer",
                >= 20 => "Newbie",
                _ => "Beginner"
            };
        }

        // B1 & B2: Browse & My Courses
        [AllowAnonymous]
        public async Task<IActionResult> Courses(string search = "")
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var courses = await _courseService.GetPublishedCoursesAsync(isAuthenticated);

            if (!string.IsNullOrEmpty(search))
                courses = courses.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                            (c.Tags != null && c.Tags.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();

            ViewBag.Search = search;
            if (isAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.UserId = user?.Id;
            }
            return View(courses);
        }

        public async Task<IActionResult> MyCourses(string search = "")
        {
            var user = await _userManager.GetUserAsync(User);
            var enrollments = await _enrollmentService.GetUserEnrollmentsAsync(user.Id);

            if (!string.IsNullOrEmpty(search))
                enrollments = enrollments.Where(e => e.Course.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.Search = search;
            ViewBag.User = user;
            ViewBag.Badge = CalculateBadge(user.TotalPoints);
            ViewBag.NextBadgePoints = user.TotalPoints < 20 ? 20 :
                                      user.TotalPoints < 40 ? 40 :
                                      user.TotalPoints < 60 ? 60 :
                                      user.TotalPoints < 80 ? 80 :
                                      user.TotalPoints < 100 ? 100 : 120;
            return View(enrollments);
        }

        // B3 & B4: Course Detail with Tabs
        [AllowAnonymous]
        public async Task<IActionResult> CourseDetail(int id, string tab = "overview")
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null || (!course.IsPublished && !User.IsInRole("Admin") && !User.IsInRole("Instructor")))
                return NotFound();

            await _courseService.IncrementViewCountAsync(id);

            string? userId = User.Identity?.IsAuthenticated == true ? (await _userManager.GetUserAsync(User))?.Id : null;
            var enrollment = userId != null ? await _enrollmentService.GetEnrollmentAsync(userId, id) : null;
            var progresses = enrollment != null ? await _progressService.GetCourseProgressAsync(userId, id) : new List<LessonProgress>();
            var reviews = await _reviewService.GetCourseReviewsAsync(id);
            var avgRating = await _reviewService.GetAverageRatingAsync(id);

            ViewBag.UserId = userId;
            ViewBag.Enrollment = enrollment;
            ViewBag.Progresses = progresses;
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = avgRating;
            ViewBag.ActiveTab = tab;

            return View(course);
        }

        // Join / Buy Course
        [HttpPost]
        public async Task<IActionResult> JoinCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null) return Json(new { success = false, message = "Course not found" });

            var existing = await _enrollmentService.GetEnrollmentAsync(user.Id, courseId);
            if (existing != null) return Json(new { success = true, message = "Already enrolled" });

            if (course.AccessRule == "OnInvitation")
                return Json(new { success = false, message = "Invitation only course." });

            if (course.AccessRule == "OnPayment" && course.Price > 0)
                return Json(new { success = false, requiresPayment = true, price = course.Price, message = "Payment required." });

            await _enrollmentService.EnrollUserAsync(user.Id, courseId);
            return Json(new { success = true, message = "Enrolled successfully!" });
        }

        // B5: Full Screen Player
        public async Task<IActionResult> Player(int lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            if (lesson == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var enrollment = await _enrollmentService.GetEnrollmentAsync(user.Id, lesson.CourseId);
            if (enrollment == null) return Forbid();

            if (enrollment.StartDate == null)
            {
                enrollment.StartDate = DateTime.UtcNow;
                enrollment.Status = "In Progress";
                await _enrollmentService.UpdateEnrollmentStatusAsync(user.Id, lesson.CourseId);
            }

            var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
            var lessons = await _lessonService.GetLessonsByCourseIdAsync(lesson.CourseId);
            var progresses = await _progressService.GetCourseProgressAsync(user.Id, lesson.CourseId);

            ViewBag.Course = course;
            ViewBag.Lessons = lessons;
            ViewBag.Progresses = progresses;
            ViewBag.Enrollment = enrollment;
            ViewBag.User = user;

            return View(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> MarkLessonComplete(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);
            await _progressService.MarkLessonCompleteAsync(user.Id, lessonId);
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            var completion = await _progressService.CalculateCourseCompletionAsync(user.Id, lesson.CourseId);
            return Json(new { success = true, completion });
        }

        // B6: Quiz Flow
        [Authorize]
        public async Task<IActionResult> TakeQuiz(int quizId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            // Load quiz with questions and options
            var quiz = await _quizService.GetQuizWithQuestionsAsync(quizId);
            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found.";
                return RedirectToAction("Courses");
            }

            // Check enrollment
            var isEnrolled = await _enrollmentService.IsUserEnrolledAsync(userId, quiz.CourseId);
            if (!isEnrolled)
            {
                TempData["Error"] = "You must be enrolled in this course to take the quiz.";
                return RedirectToAction("CourseDetail", new { id = quiz.CourseId });
            }

            // Check questions exist
            if (quiz.Questions == null || !quiz.Questions.Any())
            {
                TempData["Warning"] = "This quiz has no questions yet.";
                return RedirectToAction("CourseDetail", new { id = quiz.CourseId });
            }

            // Get attempt count
            var attemptCount = await _quizService.GetAttemptCountAsync(userId, quizId);

            ViewBag.AttemptNumber = attemptCount + 1;
            ViewBag.CourseId = quiz.CourseId;

            return View(quiz);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitQuiz(int quizId, Dictionary<int, int> answers)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                    return Json(new { success = false, message = "Please login again." });

                if (answers == null || !answers.Any())
                    return Json(new { success = false, message = "No answers submitted." });

                // Calculate score (percentage)
                var score = await _quizService.CalculateScoreAsync(quizId, answers);

                // Get attempt number
                var attemptCount = await _quizService.GetAttemptCountAsync(userId, quizId);
                var attemptNumber = attemptCount + 1;

                // Determine if passed (70% threshold)
                var passed = score >= 70;

                // Calculate points based on attempt number (only if passed)
                var pointsEarned = 0;
                if (passed)
                {
                    pointsEarned = await _quizService.CalculatePointsAsync(quizId, attemptNumber);
                }

                // Record the attempt
                var attempt = new QuizAttempt
                {
                    QuizId = quizId,
                    UserId = userId,
                    Score = score,
                    IsPassed = passed,
                    PointsEarned = pointsEarned,
                    AttemptNumber = attemptNumber,
                    AttemptDate = DateTime.UtcNow
                };

                await _quizService.RecordAttemptAsync(attempt);

                // Award points if passed
                if (passed && pointsEarned > 0)
                {
                    await _progressService.AddPointsToUserAsync(userId, pointsEarned);
                }

                // Get updated user info
                var user = await _userManager.FindByIdAsync(userId);

                return Json(new
                {
                    success = true,
                    score = score,
                    passed = passed,
                    pointsEarned = pointsEarned,
                    totalPoints = user?.TotalPoints ?? 0,
                    badge = user?.BadgeLevel ?? "Beginner",
                    attemptNumber = attemptNumber,
                    message = passed ? "Quiz passed!" : "Quiz not passed. Try again!"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SubmitQuiz Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }
        // B7: Complete Course
        [HttpPost]
        public async Task<IActionResult> CompleteCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var completion = await _progressService.CalculateCourseCompletionAsync(user.Id, courseId);
            if (completion < 100) return Json(new { success = false, message = "Complete all lessons first." });

            await _enrollmentService.MarkCourseCompletedAsync(user.Id, courseId);
            return Json(new { success = true, message = "Course completed successfully!" });
        }

        // Reviews
        [HttpPost]
        public async Task<IActionResult> AddReview(int courseId, int rating, string reviewText)
        {
            var user = await _userManager.GetUserAsync(User);
            var enrollment = await _enrollmentService.GetEnrollmentAsync(user.Id, courseId);
            if (enrollment == null) return Json(new { success = false, message = "Must be enrolled." });

            var exists = await _reviewService.HasUserReviewedAsync(user.Id, courseId);
            if (exists) return Json(new { success = false, message = "Already reviewed." });

            await _reviewService.AddReviewAsync(new CourseReview
            {
                UserId = user.Id,
                CourseId = courseId,
                Rating = rating,
                ReviewText = reviewText,
                CreatedAt = DateTime.UtcNow
            });
            return Json(new { success = true, message = "Review added!" });
        }
       
    

    }
}
