using Manhaj.DTOs;
using Manhaj.Models;

namespace Manhaj.IRepositories
{
    public interface ITeacherRepository
    {
        Teacher GetById(int id);
        bool Update(Teacher teacher, int id);
        List<Course> GetCourses(int teacherId);
        bool CreateCourse(Course course);

        
        // New methods for refactoring
        Teacher GetTeacherWithDetails(int id);

        // Recent Activities
        List<Student_Quiz> GetRecentQuizSubmissions(int teacherId);
        List<Student_Assignment> GetRecentAssignmentSubmissions(int teacherId);
    }
}
