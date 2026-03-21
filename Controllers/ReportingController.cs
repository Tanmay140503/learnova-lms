using Learnova.Models.Entities;
using Learnova.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Learnova.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class ReportingController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportingController(
            ICourseService courseService,
            IEnrollmentService enrollmentService,
            UserManager<ApplicationUser> userManager)
        {
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard(int? courseId, string status = "")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            List<Course> courses;
            if (isAdmin)
                courses = await _courseService.GetAllCoursesAsync();
            else
                courses = await _courseService.GetCoursesByInstructorAsync(currentUser.Id);

            List<CourseEnrollment> enrollments = new();

            if (courseId.HasValue)
            {
                enrollments = await _enrollmentService.GetCourseEnrollmentsAsync(courseId.Value);
            }
            else if (courses.Any())
            {
                // Get enrollments for all instructor's courses
                foreach (var course in courses)
                {
                    var courseEnrollments = await _enrollmentService.GetCourseEnrollmentsAsync(course.Id);
                    enrollments.AddRange(courseEnrollments);
                }
            }

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                enrollments = enrollments.Where(e => e.Status == status).ToList();
            }

            // Calculate stats
            var totalParticipants = enrollments.Count;
            var yetToStart = enrollments.Count(e => e.Status == "Yet to Start");
            var inProgress = enrollments.Count(e => e.Status == "In Progress");
            var completed = enrollments.Count(e => e.Status == "Completed");

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;
            ViewBag.SelectedStatus = status;
            ViewBag.TotalParticipants = totalParticipants;
            ViewBag.YetToStart = yetToStart;
            ViewBag.InProgress = inProgress;
            ViewBag.Completed = completed;

            return View(enrollments);
        }

        // Export to CSV (bonus feature)
        public async Task<IActionResult> ExportToCsv(int? courseId)
        {
            List<CourseEnrollment> enrollments;

            if (courseId.HasValue)
            {
                enrollments = await _enrollmentService.GetCourseEnrollmentsAsync(courseId.Value);
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var courses = await _courseService.GetCoursesByInstructorAsync(currentUser.Id);
                enrollments = new();

                foreach (var course in courses)
                {
                    var courseEnrollments = await _enrollmentService.GetCourseEnrollmentsAsync(course.Id);
                    enrollments.AddRange(courseEnrollments);
                }
            }

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Course,Participant,Email,Enrolled Date,Start Date,Completion %,Completed Date,Status,Time Spent (mins)");

            foreach (var enrollment in enrollments)
            {
                csv.AppendLine($"{enrollment.Course.Title},{enrollment.User.FirstName} {enrollment.User.LastName},{enrollment.User.Email},{enrollment.EnrolledDate:yyyy-MM-dd},{enrollment.StartDate:yyyy-MM-dd},{enrollment.CompletionPercentage:F2}%,{enrollment.CompletedDate:yyyy-MM-dd},{enrollment.Status},{enrollment.TimeSpentMinutes}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "course_report.csv");
        }
    }
}