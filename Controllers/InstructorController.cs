using Learnova.Data;
using Learnova.Models.Entities;
using Learnova.Models.ViewModels;
using Learnova.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using System.Text.Json;

namespace Learnova.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class InstructorController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;
        private readonly IQuizService _quizService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAntiforgery _antiforgery;

        public InstructorController(
            ICourseService courseService,
            ILessonService lessonService,
            IQuizService quizService,
            IEnrollmentService enrollmentService,
            IFileService fileService,
            UserManager<ApplicationUser> userManager,
            IAntiforgery antiforgery)
        {
            _courseService = courseService;
            _lessonService = lessonService;
            _quizService = quizService;
            _enrollmentService = enrollmentService;
            _fileService = fileService;
            _userManager = userManager;
            _antiforgery = antiforgery;
        }

        // ============ DASHBOARD ============

        public async Task<IActionResult> Dashboard(string view = "kanban", string search = "")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            List<Course> courses;

            if (isAdmin)
            {
                courses = string.IsNullOrEmpty(search)
                    ? await _courseService.GetAllCoursesAsync()
                    : await _courseService.SearchCoursesAsync(search);
            }
            else
            {
                courses = await _courseService.GetCoursesByInstructorAsync(currentUser.Id);
                if (!string.IsNullOrEmpty(search))
                {
                    courses = courses.Where(c =>
                        c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (c.Tags != null && c.Tags.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
            }

            ViewBag.View = view;
            ViewBag.Search = search;

            return View(courses);
        }

        // ============ COURSE CRUD ============

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateCourse(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Json(new { success = false, message = "Title is required" });

            var currentUser = await _userManager.GetUserAsync(User);

            var course = new Course
            {
                Title = title,
                ResponsibleUserId = currentUser.Id,
                CreatedAt = DateTime.UtcNow,
                Visibility = "Everyone",
                AccessRule = "Open"
            };

            await _courseService.CreateCourseAsync(course);

            return Json(new { success = true, courseId = course.Id });
        }

        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && course.ResponsibleUserId != currentUser.Id)
                return Forbid();

            var viewModel = new CourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ShortDescription = course.ShortDescription,
                Tags = course.Tags,
                CourseImageUrl = course.CourseImageUrl,
                Website = course.Website,
                IsPublished = course.IsPublished,
                Visibility = course.Visibility,
                AccessRule = course.AccessRule,
                Price = course.Price,
                ResponsibleUserId = course.ResponsibleUserId,
                ViewsCount = course.ViewsCount,
                TotalLessons = course.TotalLessons,
                TotalDurationMinutes = course.TotalDurationMinutes
            };

            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            ViewBag.Instructors = instructors.Union(admins).ToList();
            ViewBag.Lessons = await _lessonService.GetLessonsByCourseIdAsync(id);
            ViewBag.Quizzes = await _quizService.GetQuizzesByCourseIdAsync(id);
            ViewBag.CsrfToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(CourseViewModel model)
        {
            var course = await _courseService.GetCourseByIdAsync(model.Id);
            if (course == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && course.ResponsibleUserId != currentUser.Id)
                return Forbid();

            // Update only fields that are present
            if (!string.IsNullOrEmpty(model.Title)) course.Title = model.Title;
            if (model.Description != null) course.Description = model.Description;
            if (model.ShortDescription != null) course.ShortDescription = model.ShortDescription;
            if (model.Tags != null) course.Tags = model.Tags;
            if (model.Website != null) course.Website = model.Website;
            if (!string.IsNullOrEmpty(model.Visibility)) course.Visibility = model.Visibility;
            if (!string.IsNullOrEmpty(model.AccessRule)) course.AccessRule = model.AccessRule;
            if (model.Price.HasValue) course.Price = model.Price;
            if (model.ResponsibleUserId != null) course.ResponsibleUserId = model.ResponsibleUserId;

            // Handle image upload
            if (model.CourseImage != null && model.CourseImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(course.CourseImageUrl))
                    await _fileService.DeleteFileAsync(course.CourseImageUrl);

                course.CourseImageUrl = await _fileService.UploadFileAsync(model.CourseImage, "courses");
            }

            await _courseService.UpdateCourseAsync(course);

            TempData["Success"] = "Course updated successfully!";
            return RedirectToAction(nameof(EditCourse), new { id = course.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            return Json(new { success = result });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var result = await _courseService.TogglePublishAsync(id);
            return Json(new { success = result });
        }

        // ============ LESSONS ============

        public async Task<IActionResult> GetLessons(int courseId)
        {
            var lessons = await _lessonService.GetLessonsByCourseIdAsync(courseId);
            return PartialView("_LessonsListPartial", lessons);
        }

        public async Task<IActionResult> LessonModal(int? id, int courseId)
        {
            LessonViewModel model;

            if (id.HasValue && id.Value > 0)
            {
                var lesson = await _lessonService.GetLessonByIdAsync(id.Value);
                if (lesson == null)
                    return NotFound();

                model = new LessonViewModel
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Description = lesson.Description,
                    LessonType = lesson.LessonType,
                    VideoUrl = lesson.VideoUrl,
                    DurationMinutes = lesson.DurationMinutes,
                    DocumentUrl = lesson.DocumentUrl,
                    ImageUrl = lesson.ImageUrl,
                    AllowDownload = lesson.AllowDownload,
                    ResponsibleUserId = lesson.ResponsibleUserId,
                    CourseId = lesson.CourseId
                };
            }
            else
            {
                model = new LessonViewModel
                {
                    CourseId = courseId,
                    LessonType = "Video"
                };
            }

            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            ViewBag.Instructors = instructors.Union(admins).ToList();

            return PartialView("_LessonModalPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLesson([FromForm] LessonViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return Json(new { success = false, message = "Title is required" });

            try
            {
                Lesson lesson;

                if (model.Id > 0)
                {
                    lesson = await _lessonService.GetLessonByIdAsync(model.Id);
                    if (lesson == null)
                        return Json(new { success = false, message = "Lesson not found" });
                }
                else
                {
                    lesson = new Lesson { CourseId = model.CourseId };
                }

                lesson.Title = model.Title;
                lesson.Description = model.Description;
                lesson.LessonType = model.LessonType ?? "Video";
                lesson.ResponsibleUserId = model.ResponsibleUserId;
                lesson.AllowDownload = model.AllowDownload;

                switch (lesson.LessonType)
                {
                    case "Video":
                        lesson.VideoUrl = model.VideoUrl;
                        lesson.DurationMinutes = model.DurationMinutes ?? 0;
                        break;

                    case "Document":
                        if (model.DocumentFile != null && model.DocumentFile.Length > 0)
                        {
                            if (!string.IsNullOrEmpty(lesson.DocumentUrl))
                                await _fileService.DeleteFileAsync(lesson.DocumentUrl);

                            lesson.DocumentUrl = await _fileService.UploadFileAsync(model.DocumentFile, "documents");
                        }
                        break;

                    case "Image":
                        if (model.ImageFile != null && model.ImageFile.Length > 0)
                        {
                            if (!string.IsNullOrEmpty(lesson.ImageUrl))
                                await _fileService.DeleteFileAsync(lesson.ImageUrl);

                            lesson.ImageUrl = await _fileService.UploadFileAsync(model.ImageFile, "images");
                        }
                        break;
                }

                if (model.Id > 0)
                    await _lessonService.UpdateLessonAsync(lesson);
                else
                    await _lessonService.CreateLessonAsync(lesson);

                return Json(new { success = true, lessonId = lesson.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var result = await _lessonService.DeleteLessonAsync(id);
            return Json(new { success = result });
        }

        // ============ QUIZZES ============

        public async Task<IActionResult> GetQuizzes(int courseId)
        {
            var quizzes = await _quizService.GetQuizzesByCourseIdAsync(courseId);
            return PartialView("_QuizzesListPartial", quizzes);
        }

        public async Task<IActionResult> QuizBuilder(int? id, int courseId)
        {
            QuizViewModel model;

            if (id.HasValue && id.Value > 0)
            {
                var quiz = await _quizService.GetQuizWithQuestionsAsync(id.Value);
                if (quiz == null)
                    return NotFound();

                model = new QuizViewModel
                {
                    Id = quiz.Id,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    CourseId = quiz.CourseId,
                    FirstAttemptPoints = quiz.FirstAttemptPoints,
                    SecondAttemptPoints = quiz.SecondAttemptPoints,
                    ThirdAttemptPoints = quiz.ThirdAttemptPoints,
                    FourthAttemptPoints = quiz.FourthAttemptPoints,
                    Questions = quiz.Questions?.Select(q => new QuizQuestionViewModel
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        QuizId = q.QuizId,
                        Options = q.Options?.Select(o => new QuizOptionViewModel
                        {
                            Id = o.Id,
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect,
                            QuestionId = o.QuestionId
                        }).ToList() ?? new List<QuizOptionViewModel>()
                    }).ToList() ?? new List<QuizQuestionViewModel>()
                };
            }
            else
            {
                model = new QuizViewModel
                {
                    CourseId = courseId,
                    FirstAttemptPoints = 10,
                    SecondAttemptPoints = 7,
                    ThirdAttemptPoints = 5,
                    FourthAttemptPoints = 3,
                    Questions = new List<QuizQuestionViewModel>()
                };
            }

            ViewBag.CsrfToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuiz([FromBody] QuizViewModel model)
        {
            // Manually validate CSRF token for JSON requests
            try
            {
                await _antiforgery.ValidateRequestAsync(HttpContext);
            }
            catch
            {
                return Json(new { success = false, message = "Invalid request. Please refresh the page." });
            }

            if (string.IsNullOrWhiteSpace(model.Title))
                return Json(new { success = false, message = "Quiz title is required" });

            if (model.Questions == null || !model.Questions.Any())
                return Json(new { success = false, message = "Please add at least one question" });

            try
            {
                Quiz quiz;

                if (model.Id > 0)
                {
                    quiz = await _quizService.GetQuizWithQuestionsAsync(model.Id);
                    if (quiz == null)
                        return Json(new { success = false, message = "Quiz not found" });

                    quiz.Title = model.Title;
                    quiz.Description = model.Description;
                    quiz.FirstAttemptPoints = model.FirstAttemptPoints;
                    quiz.SecondAttemptPoints = model.SecondAttemptPoints;
                    quiz.ThirdAttemptPoints = model.ThirdAttemptPoints;
                    quiz.FourthAttemptPoints = model.FourthAttemptPoints;

                    await _quizService.UpdateQuizAsync(quiz);

                    // Delete old questions
                    var existingQuestions = await _quizService.GetQuestionsByQuizIdAsync(quiz.Id);
                    foreach (var eq in existingQuestions)
                    {
                        await _quizService.DeleteQuestionAsync(eq.Id);
                    }
                }
                else
                {
                    quiz = new Quiz
                    {
                        Title = model.Title,
                        Description = model.Description,
                        CourseId = model.CourseId,
                        FirstAttemptPoints = model.FirstAttemptPoints,
                        SecondAttemptPoints = model.SecondAttemptPoints,
                        ThirdAttemptPoints = model.ThirdAttemptPoints,
                        FourthAttemptPoints = model.FourthAttemptPoints
                    };

                    await _quizService.CreateQuizAsync(quiz);
                }

                // Add new questions
                int orderIndex = 1;
                foreach (var questionModel in model.Questions)
                {
                    if (string.IsNullOrWhiteSpace(questionModel.QuestionText))
                        continue;

                    var question = new QuizQuestion
                    {
                        QuestionText = questionModel.QuestionText,
                        QuizId = quiz.Id,
                        OrderIndex = orderIndex++
                    };
                    await _quizService.AddQuestionAsync(question);

                    if (questionModel.Options != null)
                    {
                        foreach (var optionModel in questionModel.Options)
                        {
                            if (string.IsNullOrWhiteSpace(optionModel.OptionText))
                                continue;

                            var option = new QuizOption
                            {
                                OptionText = optionModel.OptionText,
                                IsCorrect = optionModel.IsCorrect,
                                QuestionId = question.Id
                            };
                            await _quizService.AddOptionAsync(option);
                        }
                    }
                }

                return Json(new { success = true, quizId = quiz.Id, message = "Quiz saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var result = await _quizService.DeleteQuizAsync(id);
            return Json(new { success = result });
        }

        // ============ ATTENDEES ============

        public async Task<IActionResult> AddAttendees(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteUsers([FromBody] InviteUsersRequest request)
        {
            if (request.UserEmails == null || !request.UserEmails.Any())
                return Json(new { success = false, message = "No emails provided" });

            int successCount = 0;
            var failedEmails = new List<string>();

            foreach (var email in request.UserEmails)
            {
                var user = await _userManager.FindByEmailAsync(email.Trim());
                if (user != null)
                {
                    await _enrollmentService.EnrollUserAsync(user.Id, request.CourseId);
                    successCount++;
                }
                else
                {
                    failedEmails.Add(email);
                }
            }

            return Json(new
            {
                success = true,
                message = failedEmails.Any()
                    ? $"{successCount} users enrolled. {failedEmails.Count} emails not found."
                    : $"{successCount} users enrolled successfully!"
            });
        }

        public async Task<IActionResult> ContactAttendees(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
                return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;

            return View();
        }
    }

    public class InviteUsersRequest
    {
        public int CourseId { get; set; }
        public List<string> UserEmails { get; set; }
    }
}