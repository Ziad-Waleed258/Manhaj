using Manhaj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Manhaj.IRepositories;

namespace Manhaj.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IProgressRepository _progressRepository;

        public TeacherController(ITeacherRepository teacherRepository, ICourseRepository courseRepository, IProgressRepository progressRepository)
        {
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _progressRepository = progressRepository;
        }

        // GET: /Teacher/Dashboard
        public IActionResult Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var teacher = _teacherRepository.GetTeacherWithDetails(userId);

            if (teacher == null)
            {
                return NotFound();
            }

            // Calculate statistics
            ViewBag.TotalCourses = teacher.Courses?.Count ?? 0;
            ViewBag.TotalStudents = teacher.Courses?.Sum(c => c.Course_Registrations?.Count ?? 0) ?? 0;
            
            // Calculate average rating
            var allRatings = teacher.Courses?
                .Where(c => c.Ratings != null)
                .SelectMany(c => c.Ratings)
                .ToList() ?? new List<Course_Rating>();
            
            ViewBag.AverageRating = allRatings.Any() 
                ? allRatings.Average(r => Convert.ToDecimal(r.Rating)) 
                : 0m;
            
            // Total quizzes
            ViewBag.TotalQuizzes = teacher.Courses?
                .SelectMany(c => c.Lectures ?? new List<Lecture>())
                .Count(l => l.Quiz != null) ?? 0;

            // Recent Activities
            ViewBag.RecentQuizSubmissions = _teacherRepository.GetRecentQuizSubmissions(userId);
            ViewBag.RecentAssignmentSubmissions = _teacherRepository.GetRecentAssignmentSubmissions(userId);

            return View(teacher);
        }


        // GET: /Teacher/Courses
        public IActionResult Courses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var courses = _courseRepository.GetCoursesByTeacher(userId);

            return View(courses);
        }


        // GET: /Teacher/Course/{id}
        public IActionResult Course(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var course = _courseRepository.GetCourseWithDetails(id);

            if (course == null || course.TeacherID != userId)
            {
                return NotFound();
            }

            // Fetch progress for each student
            var studentProgress = new Dictionary<int, int>();
            foreach (var registration in course.Course_Registrations)
            {
                var progress = _progressRepository.GetStudentProgress(registration.StudentID, id);
                studentProgress.Add(registration.StudentID, progress);
            }
            ViewBag.StudentProgress = studentProgress;

            return View(course);
        }

        // GET: /Teacher/CreateCourse
        public IActionResult CreateCourse()
        {
            return View();
        }

        // POST: /Teacher/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCourse(Course course)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            course.TeacherID = userId;
            course.CreatedAt = DateTime.Now;

            var success = _teacherRepository.CreateCourse(course);

            if (success)
            {
                TempData["SuccessMessage"] = "تم إنشاء الدورة بنجاح!";
                return RedirectToAction("Course", new { id = course.Id });
            }
            
            return View(course);
        }

        // POST: /Teacher/DeleteCourse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCourse(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var course = _courseRepository.GetCourseWithDetails(id);
            
            if (course == null) return NotFound();
            if (course.TeacherID != userId) return Forbid();

            _courseRepository.Delete(id);
            TempData["SuccessMessage"] = "تم حذف الدورة بنجاح";
            return RedirectToAction("Dashboard");
        }

        // GET: /Teacher/EditCourse/{id}
        public IActionResult EditCourse(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var course = _courseRepository.GetById(id);
            
            if (course == null) return NotFound();
            if (course.TeacherID != userId) return Forbid();

            return View(course);
        }

        // POST: /Teacher/EditCourse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(int id, Manhaj.DTOs.CourseVM course)
        {
            if (id != course.Id) return NotFound();
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var existingCourse = _courseRepository.GetById(id);
            if (existingCourse == null) return NotFound();
            if (existingCourse.TeacherID != userId) return Forbid();

            if (ModelState.IsValid)
            {
                course.TeacherID = userId;
                _courseRepository.Update(course, id);
                TempData["SuccessMessage"] = "تم تعديل الدورة بنجاح";
                return RedirectToAction("Course", new { id = id });
            }
            
            return View(course);
        }
    }
}
