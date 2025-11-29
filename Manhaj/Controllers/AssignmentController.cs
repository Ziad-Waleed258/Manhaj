using Manhaj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Manhaj.IRepositories;
using Manhaj.Services;

namespace Manhaj.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ILectureRepository _lectureRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IFileService _fileService;

        public AssignmentController(IAssignmentRepository assignmentRepository, ILectureRepository lectureRepository, IStudentRepository studentRepository, IFileService fileService)
        {
            _assignmentRepository = assignmentRepository;
            _lectureRepository = lectureRepository;
            _studentRepository = studentRepository;
            _fileService = fileService;
        }

        // GET: /Assignment/Create?lectureId={id}
        [Authorize(Roles = "Teacher")]
        public IActionResult Create(int lectureId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var lecture = _lectureRepository.GetById(lectureId);

            if (lecture == null || lecture.Course.TeacherID != userId)
            {
                return NotFound();
            }

            // Check if assignment already exists
            if (lecture.assignment != null)
            {
                return RedirectToAction("Edit", new { id = lecture.assignment.Id });
            }

            ViewBag.LectureId = lectureId;
            ViewBag.LectureTitle = lecture.Title;
            return View();
        }

        // POST: /Assignment/Create
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Assignment assignment)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var lecture = _lectureRepository.GetById(assignment.lectureID);

            if (lecture == null || lecture.Course.TeacherID != userId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.LectureId = assignment.lectureID;
                ViewBag.LectureTitle = lecture.Title;
                return View(assignment);
            }

            var success = _assignmentRepository.Create(assignment);

            if (success)
            {
                TempData["SuccessMessage"] = "تم إنشاء الواجب بنجاح!";
                return RedirectToAction("Course", "Teacher", new { id = lecture.CourseID });
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء إنشاء الواجب. يرجى المحاولة مرة أخرى.";
                ViewBag.LectureId = assignment.lectureID;
                ViewBag.LectureTitle = lecture.Title;
                return View(assignment);
            }
        }

        // GET: /Assignment/Details/{id}
        public IActionResult Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var assignment = _assignmentRepository.GetAssignmentWithDetails(id);

            if (assignment == null)
            {
                return NotFound();
            }

            if (userRole == "Student")
            {
                // Check if student is enrolled
                var enrolledCourses = _studentRepository.GetEnrolledCourses(userId);
                if (!enrolledCourses.Any(cr => cr.CourseID == assignment.Lecture.CourseID))
                {
                    return RedirectToAction("BrowseCourses", "Student");
                }

                // Get student submission
                var submission = _assignmentRepository.GetStudentSubmission(id, userId);

                ViewBag.Submission = submission;
                return View("StudentDetails", assignment);
            }
            else if (userRole == "Teacher")
            {
                // Verify teacher owns the course
                if (assignment.Lecture.Course.TeacherID != userId)
                {
                    return Forbid();
                }

                // Get all submissions
                var submissions = _assignmentRepository.GetSubmissions(id);

                // Get all enrolled students
                var students = _studentRepository.GetStudentsByCourse(assignment.Lecture.CourseID);

                var studentStatuses = new List<dynamic>();
                foreach (var student in students)
                {
                    var submission = submissions.FirstOrDefault(s => s.StudentId == student.Id);
                    studentStatuses.Add(new
                    {
                        Student = student,
                        HasSubmission = submission != null,
                        Grade = submission?.Grade ?? 0,
                        SubmissionDate = submission?.SubmissionDate,
                        SubmissionPath = submission?.SubmissionPath
                    });
                }
                ViewBag.StudentStatuses = studentStatuses;

                ViewBag.Submissions = submissions;
                return View("TeacherDetails", assignment);
            }

            return NotFound();
        }

        // POST: /Assignment/Submit
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int assignmentId, IFormFile file)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var assignment = _assignmentRepository.GetAssignmentWithDetails(assignmentId);

            if (assignment == null)
            {
                return NotFound();
            }

            // Check enrollment
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId);
            if (!enrolledCourses.Any(cr => cr.CourseID == assignment.Lecture.CourseID))
            {
                return Forbid();
            }

            // Check deadline
            if (DateTime.Now > assignment.Deadline)
            {
                TempData["ErrorMessage"] = "عذراً، انتهى موعد تسليم الواجب.";
                return RedirectToAction("Details", new { id = assignmentId });
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "يرجى اختيار ملف للرفع.";
                return RedirectToAction("Details", new { id = assignmentId });
            }

            var filePath = await _fileService.SaveFileAsync(file, "assignments");

            var submission = _assignmentRepository.GetStudentSubmission(assignmentId, userId);

            if (submission == null)
            {
                submission = new Student_Assignment
                {
                    AssignmentId = assignmentId,
                    StudentId = userId,
                    SubmissionPath = filePath,
                    SubmissionDate = DateTime.Now,
                    Grade = 0 // Not graded yet
                };
            }
            else
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(submission.SubmissionPath))
                {
                    _fileService.DeleteFile(submission.SubmissionPath);
                }
                
                submission.SubmissionPath = filePath;
                submission.SubmissionDate = DateTime.Now;
            }

            _assignmentRepository.SaveSubmission(submission);

            TempData["SuccessMessage"] = "تم تسليم الواجب بنجاح!";
            return RedirectToAction("Details", new { id = assignmentId });
        }

        // POST: /Assignment/Grade
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public IActionResult Grade(int assignmentId, int studentId, decimal grade)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var assignment = _assignmentRepository.GetAssignmentWithDetails(assignmentId);

            if (assignment == null || assignment.Lecture.Course.TeacherID != userId)
            {
                return Forbid();
            }

            var submission = _assignmentRepository.GetStudentSubmission(assignmentId, studentId);

            if (submission == null)
            {
                return NotFound();
            }

            submission.Grade = grade;
            _assignmentRepository.SaveSubmission(submission);

            TempData["SuccessMessage"] = "تم رصد الدرجة بنجاح!";
            return RedirectToAction("Details", new { id = assignmentId });
        }
    }
}
