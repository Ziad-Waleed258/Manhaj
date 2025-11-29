using Manhaj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Manhaj.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class LectureController : Controller
    {
        private readonly ManhajDbContext _context;
        private readonly Manhaj.Services.IFileService _fileService;

        public LectureController(ManhajDbContext context, Manhaj.Services.IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /Lecture/Create?courseId={id}
        public async Task<IActionResult> Create(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Verify the course belongs to this teacher
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherID == userId);

            if (course == null)
            {
                return NotFound();
            }

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;
            return View();
        }

        // POST: /Lecture/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lecture lecture, IFormFile? file, string? materialTitle)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Verify the course belongs to this teacher
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == lecture.CourseID && c.TeacherID == userId);

            if (course == null)
            {
                return NotFound();
            }

            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            // Handle optional file upload
            if (file != null && file.Length > 0)
            {
                var filePath = await _fileService.SaveFileAsync(file, "materials");
                
                var material = new Material
                {
                    LectureID = lecture.Id,
                    Title = !string.IsNullOrEmpty(materialTitle) ? materialTitle : "ملف المحاضرة",
                    FilePath = filePath,
                    Filetype = Path.GetExtension(file.FileName).TrimStart('.')
                };

                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "تم إضافة المحاضرة بنجاح!";
            return RedirectToAction("Course", "Teacher", new { id = lecture.CourseID });
        }

        // GET: /Lecture/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .Include(l => l.Materials)
                .FirstOrDefaultAsync(l => l.Id == id && l.Course.TeacherID == userId);

            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        // POST: /Lecture/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lecture updatedLecture)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id && l.Course.TeacherID == userId);

            if (lecture == null)
            {
                return NotFound();
            }

            lecture.Title = updatedLecture.Title;
            lecture.Description = updatedLecture.Description;
            lecture.VideoUrl = updatedLecture.VideoUrl;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث المحاضرة بنجاح!";
            return RedirectToAction("Course", "Teacher", new { id = lecture.CourseID });
        }

        // POST: /Lecture/AddMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(int lectureId, string title, IFormFile file)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == lectureId && l.Course.TeacherID == userId);

            if (lecture == null)
            {
                return NotFound();
            }

            if (file != null && file.Length > 0)
            {
                var filePath = await _fileService.SaveFileAsync(file, "materials");
                
                var material = new Material
                {
                    LectureID = lectureId,
                    Title = title,
                    FilePath = filePath,
                    Filetype = Path.GetExtension(file.FileName).TrimStart('.')
                };

                _context.Materials.Add(material);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "تم رفع الملف بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "يرجى اختيار ملف للرفع.";
            }

            return RedirectToAction("Edit", new { id = lectureId });
        }

        // POST: /Lecture/DeleteMaterial/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var material = await _context.Materials
                .Include(m => m.Lecture)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(m => m.Id == id && m.Lecture.Course.TeacherID == userId);

            if (material == null)
            {
                return NotFound();
            }

            var lectureId = material.LectureID;
            
            // Delete file from disk
            _fileService.DeleteFile(material.FilePath);
            
            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الملف بنجاح!";
            return RedirectToAction("Edit", new { id = lectureId });
        }

        // POST: /Lecture/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .Include(l => l.Materials)
                .Include(l => l.assignment)
                .Include(l => l.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qu => qu.Options)
                .Include(l => l.Quiz)
                    .ThenInclude(q => q.Quizzes)
                .FirstOrDefaultAsync(l => l.Id == id && l.Course.TeacherID == userId);

            if (lecture == null)
            {
                return NotFound();
            }

            var courseId = lecture.CourseID;

            // Delete Student_Lectures (progress tracking)
            var studentLectures = await _context.Student_Lectures
                .Where(sl => sl.LectureId == id)
                .ToListAsync();
            _context.Student_Lectures.RemoveRange(studentLectures);

            // Delete quiz and related data
            if (lecture.Quiz != null)
            {
                // Delete quiz questions and options
                foreach (var question in lecture.Quiz.Questions)
                {
                    _context.Options.RemoveRange(question.Options);
                    _context.Questions.Remove(question);
                }
                
                // Delete student quiz attempts
                _context.Student_Quizzes.RemoveRange(lecture.Quiz.Quizzes);
                
                // Delete the quiz
                _context.Quizzes.Remove(lecture.Quiz);
            }

            // Delete assignment and student submissions
            if (lecture.assignment != null)
            {
                var studentAssignments = await _context.Student_Assignments
                    .Where(sa => sa.AssignmentId == lecture.assignment.Id)
                    .ToListAsync();
                _context.Student_Assignments.RemoveRange(studentAssignments);
                _context.Assignments.Remove(lecture.assignment);
            }

            // Delete materials
            foreach (var material in lecture.Materials)
            {
                _fileService.DeleteFile(material.FilePath);
            }
            _context.Materials.RemoveRange(lecture.Materials);

            // Delete the lecture
            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المحاضرة بنجاح!";
            return RedirectToAction("Course", "Teacher", new { id = courseId });
        }
    }
}
