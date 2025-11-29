using Manhaj.DTOs;
using Manhaj.IRepositories;
using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        ManhajDbContext _DB;

        public StudentRepository(ManhajDbContext dB)
        {
            _DB = dB;
        }

        //public List<AssignmentDTo> GetAssignments(int id)
        //{
            
        //}

        public StudentVM GetById(int id)
        {
            var student = _DB.Students.FirstOrDefault(s => s.Id == id);

            if (student == null)
            {
                return null;
            }

            return new StudentVM
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Phone = student.Phone,
                Level = student.Level
            };
        }

        public int GetCourseGrade(int id)
        {
            // Calculate total grade from all quizzes and assignments for the student
            // This is a simplified implementation
            var quizGrades = _DB.Student_Quizzes.Where(sq => sq.StudentId == id).Sum(sq => sq.Grade);
            var assignmentGrades = _DB.Student_Assignments.Where(sa => sa.StudentId == id).Sum(sa => sa.Grade);
            return (int)(quizGrades + assignmentGrades);
        }

        public int GetProgress(int id)
        {
            // Simplified progress calculation (e.g., percentage of completed lectures)
            // This would require more complex logic in a real scenario
            return 0; 
        }

        public List<QuizVM> GetQuizzes(int id)
        {
            // Get quizzes for courses the student is enrolled in
            var studentCourses = _DB.Course_Registrations
                .Where(cr => cr.StudentID == id)
                .Select(cr => cr.CourseID)
                .ToList();

            var quizzes = _DB.Lectures
                .Where(l => studentCourses.Contains(l.CourseID) && l.Quiz != null)
                .Select(l => new QuizVM
                {
                    Id = l.Quiz.Id,
                    Title = l.Quiz.Title,
                    NumberOfQuestions = l.Quiz.NumberOfQuestions,
                    TotalGrade = l.Quiz.TotalGrade
                })
                .ToList();

            return quizzes;
        }

        public bool RegisterCourse(int id, int courseID)
        {
            try
            {
                var registration = new Course_Registration
                {
                    StudentID = id,
                    CourseID = courseID,
                    EnrollmentDate = DateTime.Now
                };
                _DB.Course_Registrations.Add(registration);
                _DB.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SubmitAssignment(int id, int assignmentId, AssignmentVM assignment)
        {
            try
            {
                // Logic to handle assignment submission would go here
                // For now, we assume the controller handles the file upload and we just record the submission
                // This might need to be adjusted based on how the controller uses it
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SubmitQuiz(int id, int quizId, QuizVM quiz)
        {
             try
            {
                // Logic to handle quiz submission
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Update(StudentVM student, int id)
        {
            var existingStudent = _DB.Students.FirstOrDefault(s => s.Id == id);
            if (existingStudent == null) return false;

            existingStudent.FirstName = student.FirstName;
            existingStudent.LastName = student.LastName;
            existingStudent.Email = student.Email;
            existingStudent.Phone = student.Phone;
            existingStudent.Level = student.Level;

            _DB.SaveChanges();
            return true;
        }

        public Student GetStudentWithDetails(int id)
        {
            return _DB.Students
                .Include(s => s.Course_Registrations)
                    .ThenInclude(cr => cr.Course)
                        .ThenInclude(c => c.Teacher)
                .Include(s => s.Course_Registrations)
                    .ThenInclude(cr => cr.Course)
                        .ThenInclude(c => c.Lectures)
                .Include(s => s.Assignments)
                .FirstOrDefault(s => s.Id == id);
        }

        public List<Course_Registration> GetEnrolledCourses(int studentId)
        {
            return _DB.Course_Registrations
                .Where(cr => cr.StudentID == studentId)
                .Include(cr => cr.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(cr => cr.Course)
                    .ThenInclude(c => c.Lectures)
                .ToList();
        }

        public List<Quiz> GetUpcomingQuizzes(int studentId)
        {
            var enrolledCourseIds = _DB.Course_Registrations
                .Where(cr => cr.StudentID == studentId && cr.IsApproved)
                .Select(cr => cr.CourseID)
                .ToList();

            return _DB.Quizzes
                .Include(q => q.Course)
                .Where(q => (q.CourseId != null && enrolledCourseIds.Contains(q.CourseId.Value)) || 
                            (q.Lecture != null && enrolledCourseIds.Contains(q.Lecture.CourseID)))
                .Where(q => q.Deadline > DateTime.Now)
                .OrderBy(q => q.Deadline)
                .ToList();
        }

        public List<Assignment> GetUpcomingAssignments(int studentId)
        {
            var enrolledCourseIds = _DB.Course_Registrations
                .Where(cr => cr.StudentID == studentId && cr.IsApproved)
                .Select(cr => cr.CourseID)
                .ToList();

            return _DB.Assignments
                .Include(a => a.Lecture)
                .ThenInclude(l => l.Course)
                .Where(a => enrolledCourseIds.Contains(a.Lecture.CourseID))
                .Where(a => a.Deadline > DateTime.Now)
                .OrderBy(a => a.Deadline)
                .ToList();
        }

        public List<Lecture> GetRecentLectures(int studentId)
        {
            var enrolledCourseIds = _DB.Course_Registrations
                .Where(cr => cr.StudentID == studentId && cr.IsApproved)
                .Select(cr => cr.CourseID)
                .ToList();

            return _DB.Lectures
                .Include(l => l.Course)
                .Where(l => enrolledCourseIds.Contains(l.CourseID))
                .Where(l => l.CreatedAt >= DateTime.Now.AddDays(-7)) // Show lectures added in the last 7 days
                .OrderByDescending(l => l.CreatedAt)
                .ToList();
        }
        public List<Student> GetStudentsByCourse(int courseId)
        {
            return _DB.Course_Registrations
                .Where(cr => cr.CourseID == courseId)
                .Include(cr => cr.Student)
                .Select(cr => cr.Student)
                .ToList();
        }
    }
}
