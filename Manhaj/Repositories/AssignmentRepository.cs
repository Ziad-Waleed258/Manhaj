using Manhaj.IRepositories;
using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly ManhajDbContext _context;

        public AssignmentRepository(ManhajDbContext context)
        {
            _context = context;
        }

        public bool Create(Assignment assignment)
        {
            try
            {
                _context.Assignments.Add(assignment);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Delete(int id)
        {
            var assignment = _context.Assignments.Find(id);
            if (assignment == null) return false;

            _context.Assignments.Remove(assignment);
            _context.SaveChanges();
            return true;
        }

        public Assignment GetById(int id)
        {
            return _context.Assignments
                .Include(a => a.Lecture)
                .FirstOrDefault(a => a.Id == id);
        }

        public List<Student_Assignment> GetSubmissions(int assignmentId)
        {
            return _context.Student_Assignments
                .Include(sa => sa.Student)
                .Where(sa => sa.AssignmentId == assignmentId)
                .ToList();
        }

        public bool Update(Assignment assignment)
        {
            try
            {
                _context.Assignments.Update(assignment);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Assignment GetAssignmentWithDetails(int id)
        {
             return _context.Assignments
                .Include(a => a.Lecture)
                    .ThenInclude(l => l.Course)
                .FirstOrDefault(a => a.Id == id);
        }

        public Student_Assignment GetStudentSubmission(int assignmentId, int studentId)
        {
            return _context.Student_Assignments
                .FirstOrDefault(sa => sa.AssignmentId == assignmentId && sa.StudentId == studentId);
        }

        public bool SaveSubmission(Student_Assignment submission)
        {
            try
            {
                var exists = _context.Student_Assignments.Any(sa => sa.AssignmentId == submission.AssignmentId && sa.StudentId == submission.StudentId);

                if (!exists)
                {
                    _context.Student_Assignments.Add(submission);
                }
                else
                {
                    _context.Student_Assignments.Update(submission);
                }
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
