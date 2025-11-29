using Manhaj.DTOs;
using Manhaj.Models;

namespace Manhaj.IRepositories
{
    public interface IStudentRepository
    {
        public StudentVM GetById(int id);

        public bool Update(StudentVM student , int id);

        //public List<CourseVM> GetCourses(int id);

        public bool RegisterCourse(int id, int courseID);

        public List<QuizVM> GetQuizzes(int id);

        public bool SubmitQuiz(int id, int quizId, QuizVM quiz);

        //public List<AssignmentVM> GetAssignments(int id);

        public bool SubmitAssignment(int id, int assignmentId, AssignmentVM assignment);

        public int GetProgress(int id);
        public int GetCourseGrade(int id);

        // New methods for refactoring
        public Student GetStudentWithDetails(int id);
        public List<Course_Registration> GetEnrolledCourses(int studentId);
        
        // Recent Activities
        public List<Quiz> GetUpcomingQuizzes(int studentId);
        public List<Assignment> GetUpcomingAssignments(int studentId);
        public List<Lecture> GetRecentLectures(int studentId);
        
        public List<Student> GetStudentsByCourse(int courseId);
    }
}
