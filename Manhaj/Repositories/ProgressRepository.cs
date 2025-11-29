using Manhaj.IRepositories;
using Manhaj.Models;

namespace Manhaj.Repositories
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly ManhajDbContext _context;

        public ProgressRepository(ManhajDbContext context)
        {
            _context = context;
        }

        public int GetStudentProgress(int studentId, int courseId)
        {
            var totalLectures = _context.Lectures.Count(l => l.CourseID == courseId);
            if (totalLectures == 0) return 0;

            var completedLectures = _context.Student_Lectures
                .Count(sl => sl.StudentId == studentId && sl.Lecture.CourseID == courseId && sl.IsCompleted);

            return (int)((double)completedLectures / totalLectures * 100);
        }

        public List<int> GetCompletedLectures(int studentId, int courseId)
        {
            return _context.Student_Lectures
                .Where(sl => sl.StudentId == studentId && sl.Lecture.CourseID == courseId && sl.IsCompleted)
                .Select(sl => sl.LectureId)
                .ToList();
        }

        public bool ToggleProgress(int studentId, int lectureId)
        {
            var studentLecture = _context.Student_Lectures
                .FirstOrDefault(sl => sl.StudentId == studentId && sl.LectureId == lectureId);

            if (studentLecture == null)
            {
                studentLecture = new Student_Lecture
                {
                    StudentId = studentId,
                    LectureId = lectureId,
                    IsCompleted = true,
                    CompletedAt = DateTime.Now
                };
                _context.Student_Lectures.Add(studentLecture);
            }
            else
            {
                studentLecture.IsCompleted = !studentLecture.IsCompleted;
                studentLecture.CompletedAt = studentLecture.IsCompleted ? DateTime.Now : null;
                _context.Student_Lectures.Update(studentLecture);
            }

            return _context.SaveChanges() > 0;
        }
    }
}
