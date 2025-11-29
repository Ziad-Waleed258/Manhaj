using Manhaj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Manhaj.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IProgressRepository _progressRepository;



        // GET: /Student/Dashboard
        public IActionResult Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var student = _studentRepository.GetStudentWithDetails(userId);

            if (student == null)
            {
                return NotFound();
            }

            // Calculate statistics
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId) ?? new List<Course_Registration>();
            ViewBag.TotalEnrolledCourses = enrolledCourses.Count;
            
            // Completed courses (100% progress)
            ViewBag.CompletedCourses = enrolledCourses.Count(c => 
                _progressRepository.GetStudentProgress(userId, c.CourseID) == 100);
            
            // Total completed lectures
            ViewBag.CompletedLectures = enrolledCourses.Sum(c => 
                _progressRepository.GetCompletedLectures(userId, c.CourseID)?.Count ?? 0);
            
            // Average quiz score - simplified
            // Average quiz score - simplified
            ViewBag.AverageQuizScore = 0m;

            // Recent Activities - Merged and Sorted
            var activities = new List<Manhaj.ViewModels.ActivityVM>();

            var upcomingQuizzes = _studentRepository.GetUpcomingQuizzes(userId);
            foreach (var q in upcomingQuizzes)
            {
                activities.Add(new Manhaj.ViewModels.ActivityVM
                {
                    Type = "Quiz",
                    Title = q.Title,
                    CourseTitle = q.Course?.Title ?? q.Lecture?.Course?.Title ?? "غير محدد",
                    Date = q.Deadline ?? q.StartTime.AddMinutes(q.Duration), // Use Deadline or calc end time
                    Link = Url.Action("Details", "Quiz", new { id = q.Id }),
                    LinkText = "بدء الاختبار"
                });
            }

            var upcomingAssignments = _studentRepository.GetUpcomingAssignments(userId);
            foreach (var a in upcomingAssignments)
            {
                activities.Add(new Manhaj.ViewModels.ActivityVM
                {
                    Type = "Assignment",
                    Title = a.Title,
                    CourseTitle = a.Lecture?.Course?.Title ?? "غير محدد",
                    Date = a.Deadline,
                    Link = Url.Action("Details", "Assignment", new { id = a.Id }),
                    LinkText = "عرض الواجب"
                });
            }

            var recentLectures = _studentRepository.GetRecentLectures(userId);
            foreach (var l in recentLectures)
            {
                activities.Add(new Manhaj.ViewModels.ActivityVM
                {
                    Type = "Lecture",
                    Title = l.Title,
                    CourseTitle = l.Course?.Title ?? "غير محدد",
                    Date = l.CreatedAt,
                    Link = l.VideoUrl,
                    LinkText = "مشاهدة"
                });
            }

            // Sort by Date Descending (Most recent/upcoming first)
            // Or maybe separate logic? User said "recent in the top". 
            // If we mix future deadlines and past created dates, "most recent" usually means "closest to now" or "newest created".
            // For a feed, usually "Newest Created" is best. But Quizzes have Deadlines.
            // Let's sort by Date Descending. Future dates (deadlines) will be at top, then recent lectures.
            ViewBag.Activities = activities.OrderByDescending(a => a.Date).ToList();

            return View(student);
        }

        // GET: /Student/Courses
        public IActionResult Courses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId);

            return View(enrolledCourses);
        }

        // GET: /Student/Course/{id}
        public IActionResult Course(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Check if student is enrolled
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId);
            var enrollment = enrolledCourses.FirstOrDefault(cr => cr.CourseID == id);

            var course = _courseRepository.GetCourseWithDetails(id);

            if (course == null)
            {
                return NotFound();
            }

            // Fetch progress data
            if (enrollment != null)
            {
                ViewBag.EnrollmentStatus = enrollment.IsApproved ? "Approved" : enrollment.PaymentStatus == "Pending" ? "Pending" : "Rejected";
                
                if (enrollment.IsApproved)
                {
                    ViewBag.ProgressPercentage = _progressRepository.GetStudentProgress(userId, id);
                    ViewBag.CompletedLectures = _progressRepository.GetCompletedLectures(userId, id);
                }
                else
                {
                    ViewBag.ProgressPercentage = 0;
                    ViewBag.CompletedLectures = new List<int>();
                }
            }
            else
            {
                ViewBag.EnrollmentStatus = "None";
                ViewBag.ProgressPercentage = 0;
                ViewBag.CompletedLectures = new List<int>();
            }

            return View(course);
        }

        // GET: /Student/BrowseCourses
        public IActionResult BrowseCourses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var student = _studentRepository.GetStudentWithDetails(userId);

            var courses = _courseRepository.GetAllCoursesWithDetails()
                .Where(c => c.level == student.Level)
                .Select(c => new Course
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    level = c.level,
                    Price = c.Price,
                    Teacher = c.Teacher,
                    Lectures = c.Lectures,
                    Course_Registrations = c.Course_Registrations ?? new List<Course_Registration>()
                })
                .ToList();

            return View(courses);
        }

        // POST: /Student/Enroll/{courseId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enroll(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if already enrolled
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId);
            var existingEnrollment = enrolledCourses.FirstOrDefault(cr => cr.CourseID == courseId);
            
            if (existingEnrollment != null)
            {
                if (existingEnrollment.IsApproved)
                {
                    TempData["ErrorMessage"] = "أنت مسجل بالفعل في هذه الدورة";
                }
                else if (existingEnrollment.PaymentStatus == "Pending")
                {
                    TempData["ErrorMessage"] = "لديك طلب تسجيل معلق لهذه الدورة";
                }
                else
                {
                    // If rejected, maybe allow re-enrollment? For now just show message
                    TempData["ErrorMessage"] = "تم رفض طلب تسجيلك السابق. يرجى التواصل مع الإدارة";
                }
                return RedirectToAction("Course", new { id = courseId });
            }

            var success = _studentRepository.RegisterCourse(userId, courseId);
            
            if (success)
            {
                TempData["SuccessMessage"] = "تم التسجيل في الدورة بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء التسجيل";
            }

            return RedirectToAction("Course", new { id = courseId });
        }

        [HttpPost]
        public IActionResult ToggleLectureCompletion(int lectureId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var success = _progressRepository.ToggleProgress(userId, lectureId);
            
            if (success)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Failed to update progress" });
        }

        // GET: /Student/PurchaseCourse/{courseId}
        public IActionResult PurchaseCourse(int courseId)
        {
            var course = _courseRepository.GetCourseWithDetails(courseId);
            
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: /Student/PurchaseCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PurchaseCourse(int courseId, IFormFile paymentProof)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if already enrolled or pending
            var existingEnrollment = await _context.Course_Registrations
                .FirstOrDefaultAsync(cr => cr.StudentID == userId && cr.CourseID == courseId);

            if (existingEnrollment != null)
            {
                TempData["ErrorMessage"] = "لديك طلب تسجيل قائم بالفعل في هذه الدورة";
                return RedirectToAction("Course", new { id = courseId });
            }

            if (paymentProof == null || paymentProof.Length == 0)
            {
                TempData["ErrorMessage"] = "يرجى رفع إثبات الدفع";
                return RedirectToAction("PurchaseCourse", new { courseId = courseId });
            }

            // Save payment proof image
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "payments");
            Directory.CreateDirectory(uploadsFolder);
            
            var uniqueFileName = $"{userId}_{courseId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(paymentProof.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await paymentProof.CopyToAsync(fileStream);
            }

            // Create enrollment record with pending status
            var enrollment = new Course_Registration
            {
                StudentID = userId,
                CourseID = courseId,
                Registration_Date = DateTime.Now,
                Progress = 0,
                Grade = 0,
                IsApproved = false,
                PaymentStatus = "Pending",
                PaymentProofPath = $"/uploads/payments/{uniqueFileName}"
            };

            _context.Course_Registrations.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إرسال طلب الشراء بنجاح! سيتم مراجعته من قبل المسؤول";
            return RedirectToAction("Dashboard");
        }

        private readonly ManhajDbContext _context;

        public StudentController(IStudentRepository studentRepository, ICourseRepository courseRepository, IProgressRepository progressRepository, ManhajDbContext context)
        {
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _progressRepository = progressRepository;
            _context = context;
        }
    }
}
