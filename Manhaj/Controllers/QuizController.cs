using Manhaj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Manhaj.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly ManhajDbContext _context;

        public QuizController(ManhajDbContext context)
        {
            _context = context;
        }

        // GET: Quiz/Create?lectureId=5
        [Authorize(Policy = "TeacherOnly")]
        public async Task<IActionResult> Create(int lectureId)
        {
            // Check if quiz already exists
            var existingQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.LectureId == lectureId);
            if (existingQuiz != null)
            {
                return RedirectToAction("Details", new { id = existingQuiz.Id });
            }

            var lecture = await _context.Lectures.FindAsync(lectureId);
            if (lecture != null)
            {
                ViewBag.CourseId = lecture.CourseID;
            }

            ViewBag.LectureId = lectureId;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "TeacherOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quiz quiz)
        {
            // Check if quiz already exists
            var existingQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.LectureId == quiz.LectureId);
            if (existingQuiz != null)
            {
                return RedirectToAction("Details", new { id = existingQuiz.Id });
            }

            if (ModelState.IsValid)
            {
                // Ensure EndTime is after StartTime
                if (quiz.EndTime <= quiz.StartTime)
                {
                    ModelState.AddModelError("EndTime", "وقت الانتهاء يجب أن يكون بعد وقت البدء");
                }
                else
                {
                    quiz.Deadline = quiz.EndTime; // Sync for compatibility
                    _context.Quizzes.Add(quiz);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = quiz.Id });
                }
            }
            
            var lecture = await _context.Lectures.FindAsync(quiz.LectureId);
            if (lecture != null)
            {
                ViewBag.CourseId = lecture.CourseID;
            }
            
            ViewBag.LectureId = quiz.LectureId;
            return View(quiz);
        }

        // GET: Quiz/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Options)
                .Include(q => q.Lecture)
                .Include(q => q.Quizzes)
                    .ThenInclude(sq => sq.Student)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Student")
            {
                // Check if student already has an attempt record
                var attempt = await _context.Student_Quizzes
                    .Include(sq => sq.Quiz)
                        .ThenInclude(q => q.Lecture)
                    .FirstOrDefaultAsync(sq => sq.QuizId == id && sq.StudentId == userId);

                if (attempt != null)
                {
                    if (attempt.IsSubmitted)
                    {
                        return View("Result", attempt);
                    }
                    else
                    {
                        // Resume quiz if not submitted
                        return RedirectToAction("Take", new { id = id });
                    }
                }
                
                // Check if entry deadline has passed
                if (DateTime.Now > quiz.EndTime)
                {
                    TempData["ErrorMessage"] = "لقد انتهى موعد دخول الاختبار";
                    var registration = await _context.Course_Registrations
                        .Include(cr => cr.Course)
                        .FirstOrDefaultAsync(cr => cr.StudentID == userId && cr.CourseID == quiz.Lecture.CourseID);
                    return RedirectToAction("Course", "Student", new { id = registration?.CourseID });
                }
                
                // Show "Start Quiz" page
                return View(quiz);
            }

            // Teacher View: Fetch all students and their status
            var courseId = quiz.Lecture.CourseID;
            var students = await _context.Course_Registrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseID == courseId)
                .Select(cr => cr.Student)
                .ToListAsync();

            var attempts = await _context.Student_Quizzes
                .Where(sq => sq.QuizId == id)
                .ToListAsync();

            var studentStatuses = new List<dynamic>();
            foreach (var student in students)
            {
                var attempt = attempts.FirstOrDefault(a => a.StudentId == student.Id);
                studentStatuses.Add(new
                {
                    Student = student,
                    HasAttempt = attempt != null,
                    IsSubmitted = attempt?.IsSubmitted ?? false,
                    Grade = attempt?.Grade ?? 0,
                    SubmissionDate = attempt?.SubmissionDate
                });
            }
            ViewBag.StudentStatuses = studentStatuses;

            return View(quiz); // Teacher view
        }

        // POST: Quiz/Start/5
        [HttpPost]
        [Authorize(Policy = "StudentOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if attempt exists
            var attempt = await _context.Student_Quizzes
                .FirstOrDefaultAsync(sq => sq.QuizId == id && sq.StudentId == userId);

            if (attempt == null)
            {
                // Check deadlines
                if (DateTime.Now < quiz.StartTime || DateTime.Now > quiz.EndTime)
                {
                    TempData["ErrorMessage"] = "الاختبار غير متاح حالياً";
                    return RedirectToAction("Details", new { id = id });
                }

                // Create new attempt
                attempt = new Student_Quiz
                {
                    StudentId = userId,
                    QuizId = id,
                    StartTime = DateTime.Now,
                    SubmissionDate = DateTime.Now, // Placeholder
                    IsSubmitted = false,
                    Grade = 0
                };
                _context.Student_Quizzes.Add(attempt);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Take", new { id = id });
        }

        // GET: Quiz/Take/5
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var attempt = await _context.Student_Quizzes
                .FirstOrDefaultAsync(sq => sq.QuizId == id && sq.StudentId == userId);

            if (attempt == null)
            {
                return RedirectToAction("Details", new { id = id });
            }

            if (attempt.IsSubmitted)
            {
                return RedirectToAction("Details", new { id = id });
            }

            // Calculate remaining time
            var timePassed = DateTime.Now - attempt.StartTime;
            var remainingSeconds = (quiz.Duration * 60) - timePassed.TotalSeconds;

            if (remainingSeconds <= 0)
            {
                // Time expired, force submit
                return await Submit(id, new Dictionary<int, int>());
            }

            ViewBag.RemainingSeconds = (int)remainingSeconds;
            return View(quiz);
        }

        // GET: Quiz/AddQuestion/5
        [Authorize(Policy = "TeacherOnly")]
        public IActionResult AddQuestion(int quizId)
        {
            ViewBag.QuizId = quizId;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "TeacherOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(int quizId, string content, string trueAnswer, decimal points, List<string> options)
        {
            // Check if quiz has already started
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null)
            {
                return NotFound();
            }

            if (DateTime.Now >= quiz.StartTime)
            {
                TempData["ErrorMessage"] = "لا يمكن إضافة أسئلة بعد بدء الاختبار";
                return RedirectToAction("Details", new { id = quizId });
            }

            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(trueAnswer) || options == null || options.Count < 2)
            {
                ModelState.AddModelError("", "Please provide question content, correct answer, and at least 2 options.");
                ViewBag.QuizId = quizId;
                return View();
            }

            var question = new Question
            {
                QuizId = quizId,
                Content = content,
                TrueAnswer = trueAnswer,
                Points = points,
                Options = new List<Option>()
            };

            foreach (var optText in options)
            {
                if (!string.IsNullOrWhiteSpace(optText))
                {
                    question.Options.Add(new Option { Content = optText });
                }
            }

            _context.Questions.Add(question);
            
            // Update Quiz stats - reuse the quiz variable from earlier
            if (quiz != null)
            {
                quiz.NumberOfQuestions++;
                quiz.TotalGrade += points;
                _context.Quizzes.Update(quiz);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = quizId });
        }

        // GET: Quiz/CreateWizard?lectureId=5
        [Authorize(Policy = "TeacherOnly")]
        public async Task<IActionResult> CreateWizard(int lectureId)
        {
            // Check if quiz already exists
            var existingQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.LectureId == lectureId);
            if (existingQuiz != null)
            {
                return RedirectToAction("Details", new { id = existingQuiz.Id });
            }

            var lecture = await _context.Lectures.FindAsync(lectureId);
            if (lecture != null)
            {
                ViewBag.CourseId = lecture.CourseID;
            }

            return View(new Manhaj.ViewModels.QuizWizardVM { LectureId = lectureId });
        }

        [HttpPost]
        [Authorize(Policy = "TeacherOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWizard(Manhaj.ViewModels.QuizWizardVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Ensure EndTime is after StartTime
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime", "وقت الانتهاء يجب أن يكون بعد وقت البدء");
                return View(model);
            }

            // Create Quiz
            var quiz = new Quiz
            {
                Title = model.Title,
                LectureId = model.LectureId,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Duration = model.Duration,
                Deadline = model.EndTime, // Sync for compatibility
                NumberOfQuestions = model.Questions.Count,
                TotalGrade = model.Questions.Sum(q => q.Points),
                Questions = new List<Question>()
            };

            // Create Questions
            foreach (var qVM in model.Questions)
            {
                var question = new Question
                {
                    Content = qVM.Content,
                    TrueAnswer = qVM.TrueAnswer,
                    Points = qVM.Points,
                    Options = new List<Option>()
                };

                foreach (var optText in qVM.Options)
                {
                    if (!string.IsNullOrWhiteSpace(optText))
                    {
                        question.Options.Add(new Option { Content = optText });
                    }
                }

                quiz.Questions.Add(question);
            }

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إنشاء الاختبار بنجاح";
            return RedirectToAction("Details", new { id = quiz.Id });
        }

        // POST: Quiz/DeleteQuestion/5
        [HttpPost]
        [Authorize(Policy = "TeacherOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int questionId, int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null)
            {
                return NotFound();
            }

            // Time check removed per user request
            /*
            if (DateTime.Now >= quiz.StartTime)
            {
                TempData["ErrorMessage"] = "لا يمكن حذف الأسئلة بعد بدء الاختبار";
                return RedirectToAction("Details", new { id = quizId });
            }
            */

            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                return NotFound();
            }

            // Update quiz stats
            quiz.NumberOfQuestions--;
            quiz.TotalGrade -= question.Points;

            _context.Questions.Remove(question);
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف السؤال بنجاح";
            return RedirectToAction("Details", new { id = quizId });
        }

        // GET: Quiz/EditQuestion/5
        [Authorize(Policy = "TeacherOnly")]
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            // Time check removed per user request
            /*
            if (DateTime.Now >= question.Quiz.StartTime)
            {
                TempData["ErrorMessage"] = "لا يمكن تعديل الأسئلة بعد بدء الاختبار";
                return RedirectToAction("Details", new { id = question.QuizId });
            }
            */

            return View(question);
        }

        // POST: Quiz/EditQuestion/5
        [HttpPost]
        [Authorize(Policy = "TeacherOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(int id, string content, string trueAnswer, decimal points, List<string> options)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Quiz)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            // Time check removed per user request
            /*
            if (DateTime.Now >= question.Quiz.StartTime)
            {
                TempData["ErrorMessage"] = "لا يمكن تعديل الأسئلة بعد بدء الاختبار";
                return RedirectToAction("Details", new { id = question.QuizId });
            }
            */

            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(trueAnswer) || options == null || options.Count < 2)
            {
                ModelState.AddModelError("", "Please provide question content, correct answer, and at least 2 options.");
                return View(question);
            }

            // Update quiz total grade
            var quiz = question.Quiz;
            quiz.TotalGrade = quiz.TotalGrade - question.Points + points;

            // Update question
            question.Content = content;
            question.TrueAnswer = trueAnswer;
            question.Points = points;

            // Remove old options
            _context.Options.RemoveRange(question.Options);

            // Add new options
            question.Options = new List<Option>();
            foreach (var optText in options)
            {
                if (!string.IsNullOrWhiteSpace(optText))
                {
                    question.Options.Add(new Option { Content = optText });
                }
            }

            _context.Questions.Update(question);
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تعديل السؤال بنجاح";
            return RedirectToAction("Details", new { id = question.QuizId });
        }

        // POST: Quiz/Submit/5
        [HttpPost]
        [Authorize(Policy = "StudentOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, Dictionary<int, int> answers)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var attempt = await _context.Student_Quizzes
                .FirstOrDefaultAsync(sq => sq.QuizId == id && sq.StudentId == userId);

            if (attempt == null)
            {
                return RedirectToAction("Details", new { id = id });
            }

            if (attempt.IsSubmitted)
            {
                return RedirectToAction("Details", new { id = id });
            }

            // Calculate score based on points and save answers
            decimal score = 0;
            int answersAdded = 0;
            if (answers != null)
            {
                foreach (var question in quiz.Questions)
                {
                    if (answers.ContainsKey(question.Id))
                    {
                        var selectedOptionId = answers[question.Id];
                        var selectedOption = question.Options.FirstOrDefault(o => o.Id == selectedOptionId);

                        if (selectedOption != null)
                        {
                            // Save the answer
                            var answerRecord = new Student_Quiz_Answer
                            {
                                StudentId = userId,
                                QuizId = id,
                                QuestionId = question.Id,
                                SelectedOptionId = selectedOptionId
                            };
                            _context.Student_Quiz_Answers.Add(answerRecord);
                            answersAdded++;

                            // Check if correct
                            if (selectedOption.Content == question.TrueAnswer)
                            {
                                score += question.Points;
                            }
                        }
                    }
                }
            }

            attempt.Grade = score;
            attempt.SubmissionDate = DateTime.Now;
            attempt.IsSubmitted = true;

            _context.Student_Quizzes.Update(attempt);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Quiz/ViewAnswers/5
        [Authorize(Policy = "StudentOnly")]
        public async Task<IActionResult> ViewAnswers(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get student's attempt
            var attempt = await _context.Student_Quizzes
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qu => qu.Options)
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Lecture) // Include Lecture to access CourseID
                .Include(sq => sq.Answers)
                .FirstOrDefaultAsync(sq => sq.QuizId == id && sq.StudentId == userId);

            if (attempt == null)
            {
                return NotFound();
            }

            // Check if quiz period has ended (EndTime + Duration)
            // This ensures all students have finished
            var quizEndTime = attempt.Quiz.EndTime.AddMinutes(attempt.Quiz.Duration);
            
            if (DateTime.Now < quizEndTime)
            {
                TempData["ErrorMessage"] = "لا يمكن عرض الإجابات إلا بعد انتهاء وقت الاختبار للجميع";
                return RedirectToAction("Details", new { id = id });
            }

            // Explicitly load answers if not already loaded
            if (attempt.Answers == null || !attempt.Answers.Any())
            {
                attempt.Answers = await _context.Student_Quiz_Answers
                    .Where(sqa => sqa.StudentId == userId && sqa.QuizId == id)
                    .Include(sqa => sqa.SelectedOption)
                    .ToListAsync();
            }

            return View(attempt);
        }

        // GET: Quiz/ViewStudentAnswers/5?studentId=3
        [Authorize(Policy = "TeacherOnly")]
        public async Task<IActionResult> ViewStudentAnswers(int id, int studentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get quiz and verify teacher owns it
            var quiz = await _context.Quizzes
                .Include(q => q.Lecture)
                    .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null || quiz.Lecture.Course.TeacherID != userId)
            {
                return Forbid();
            }

            // Get student's attempt with answers
            var attempt = await _context.Student_Quizzes
                .Include(sq => sq.Student)
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qu => qu.Options)
                .Include(sq => sq.Quiz)
                    .ThenInclude(q => q.Lecture)
                .Include(sq => sq.Answers)
                    .ThenInclude(a => a.SelectedOption)
                .FirstOrDefaultAsync(sq => sq.QuizId == id && sq.StudentId == studentId);

            if (attempt == null)
            {
                TempData["ErrorMessage"] = "لم يقم هذا الطالب بحل الاختبار";
                return RedirectToAction("Details", new { id = id });
            }

            // Explicitly load answers if not already loaded
            if (attempt.Answers == null || !attempt.Answers.Any())
            {
                attempt.Answers = await _context.Student_Quiz_Answers
                    .Where(sqa => sqa.StudentId == studentId && sqa.QuizId == id)
                    .Include(sqa => sqa.SelectedOption)
                    .ToListAsync();
            }

            return View("ViewAnswers", attempt);
        }
    }
}
