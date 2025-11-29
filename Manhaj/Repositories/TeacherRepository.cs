using Manhaj.IRepositories;
using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly ManhajDbContext _context;

        public TeacherRepository(ManhajDbContext context)
        {
            _context = context;
        }

        public bool CreateCourse(Course course)
        {
            try
            {
                _context.Courses.Add(course);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Teacher GetById(int id)
        {
            return _context.Teachers.FirstOrDefault(t => t.Id == id);
        }

        public List<Course> GetCourses(int teacherId)
        {
            return _context.Courses
                .Where(c => c.TeacherID == teacherId)
                .Include(c => c.Lectures)
                .Include(c => c.Course_Registrations)
                .ToList();
        }

        public bool Update(Teacher teacher, int id)
        {
            var existingTeacher = _context.Teachers.FirstOrDefault(t => t.Id == id);
            if (existingTeacher == null) return false;

            existingTeacher.FirstName = teacher.FirstName;
            existingTeacher.LastName = teacher.LastName;
            existingTeacher.Email = teacher.Email;
            existingTeacher.Phone = teacher.Phone;
            existingTeacher.Specialization = teacher.Specialization;

            _context.SaveChanges();
            return true;
        }

        public Teacher GetTeacherWithDetails(int id)
        {
            return _context.Teachers
                .Include(t => t.Courses)
                    .ThenInclude(c => c.Course_Registrations)
                .Include(t => t.Courses)
                    .ThenInclude(c => c.Lectures)
                .FirstOrDefault(t => t.Id == id);
        }

        public List<Student_Quiz> GetRecentQuizSubmissions(int teacherId)
        {
            // Get submissions for quizzes in courses taught by this teacher, where the quiz deadline hasn't passed yet
            // Note: The requirement was "show if a student handed in... until the deadline of the quiz"
            return _context.Student_Quizzes
                .Include(sq => sq.Student)
                .Include(sq => sq.Quiz)
                .ThenInclude(q => q.Course)
                .Where(sq => sq.Quiz.Course.TeacherID == teacherId || sq.Quiz.Lecture.Course.TeacherID == teacherId)
                .Where(sq => sq.Quiz.Deadline > DateTime.Now)
                .OrderByDescending(sq => sq.SubmissionDate)
                .ToList();
        }

        public List<Student_Assignment> GetRecentAssignmentSubmissions(int teacherId)
        {
            return _context.Student_Assignments
                .Include(sa => sa.Student)
                .Include(sa => sa.Assignment)
                .ThenInclude(a => a.Lecture)
                .ThenInclude(l => l.Course)
                .Where(sa => sa.Assignment.Lecture.Course.TeacherID == teacherId)
                .Where(sa => sa.Assignment.Deadline > DateTime.Now)
                .OrderByDescending(sa => sa.SubmissionDate)
                .ToList();
        }
    }
}
