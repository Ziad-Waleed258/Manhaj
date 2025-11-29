using Manhaj.DTOs;
using Manhaj.Models;

namespace Manhaj.IRepositories
{
    public interface IAssignmentRepository
    {
        Assignment GetById(int id);
        bool Create(Assignment assignment);
        bool Update(Assignment assignment);
        bool Delete(int id);
        List<Student_Assignment> GetSubmissions(int assignmentId);


        // New methods for refactoring
        Assignment GetAssignmentWithDetails(int id);
        Student_Assignment GetStudentSubmission(int assignmentId, int studentId);
        bool SaveSubmission(Student_Assignment submission);
    }
}
