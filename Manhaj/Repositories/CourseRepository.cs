using Manhaj.DTOs;
using Manhaj.IRepositories;
using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        ManhajDbContext _DB;

        public CourseRepository(ManhajDbContext dB)
        {
            _DB = dB;
        }



        public void Create(CourseVM courseDto)
        {
            var course = new Course
            {
                Title = courseDto.Title,
                Description = courseDto.Description,
                level = courseDto.level,
                Price = courseDto.Price,
                Duration = courseDto.Duration,
                Num_Of_Lectures = courseDto.Num_Of_Lectures,
                TeacherID = courseDto.TeacherID,
                Creation_Date = DateTime.UtcNow
            };
            _DB.Courses.Add(course);
            _DB.SaveChanges();
        }

        public bool Delete(int id)
        {
            Course Course = _DB.Courses
                .Include(c => c.Course_Registrations)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Materials)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.assignment)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                        .ThenInclude(q => q.Questions)
                            .ThenInclude(qu => qu.Options)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                        .ThenInclude(q => q.Quizzes)
                .FirstOrDefault(c => c.Id == id);
                
            if (Course == null)
            {
                return false;
            }

            // Delete all related entities
            foreach (var lecture in Course.Lectures.ToList())
            {
                // Delete student lecture progress
                var studentLectures = _DB.Student_Lectures.Where(sl => sl.LectureId == lecture.Id);
                _DB.Student_Lectures.RemoveRange(studentLectures);

                // Delete quiz and its questions/options
                if (lecture.Quiz != null)
                {
                    foreach (var question in lecture.Quiz.Questions.ToList())
                    {
                        _DB.Options.RemoveRange(question.Options);
                        _DB.Questions.Remove(question);
                    }
                    _DB.Student_Quizzes.RemoveRange(lecture.Quiz.Quizzes);
                    _DB.Quizzes.Remove(lecture.Quiz);
                }

                // Delete assignment
                if (lecture.assignment != null)
                {
                    var studentAssignments = _DB.Student_Assignments.Where(sa => sa.AssignmentId == lecture.assignment.Id);
                    _DB.Student_Assignments.RemoveRange(studentAssignments);
                    _DB.Assignments.Remove(lecture.assignment);
                }

                // Delete materials
                _DB.Materials.RemoveRange(lecture.Materials);
                
                // Delete lecture
                _DB.Lectures.Remove(lecture);
            }

            // Delete course registrations
            _DB.Course_Registrations.RemoveRange(Course.Course_Registrations);

            // Delete the course itself
            _DB.Courses.Remove(Course);
            _DB.SaveChanges();
            return true;
        }

        public List<CourseVM> GetAll()
        {
            var courses = _DB.Courses
                 .Select(c => new CourseVM
                 {
                     Id = c.Id,
                     Title = c.Title,
                     Description = c.Description,
                     level = c.level,
                     Price = c.Price,
                     Duration = c.Duration,
                     Num_Of_Lectures = c.Num_Of_Lectures,
                     Creation_Date = c.Creation_Date,
                     TeacherID = c.TeacherID,
                 })
                 .ToList();
            return courses;
        }

        public CourseVM GetById(int id)
        {
            var course = _DB.Courses.FirstOrDefault(c => c.Id == id);

            if (course is null)
            {
                return null;
            }

            return new CourseVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                level = course.level,
                Price = course.Price,
                Duration = course.Duration,
                Num_Of_Lectures = course.Num_Of_Lectures,
                Creation_Date = course.Creation_Date,
                TeacherID = course.TeacherID
            };
        }



        public CourseVM GetByName(string name)
        {
            var course = _DB.Courses.FirstOrDefault(c => c.Title == name);

            if (course is null)
            {
                return null;
            }

            return new CourseVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                level = course.level,
                Price = course.Price,
                Duration = course.Duration,
                Num_Of_Lectures = course.Num_Of_Lectures,
                Creation_Date = course.Creation_Date,
                TeacherID = course.TeacherID
            };
        }

        public bool Update(CourseVM courseDto, int id)
        {
            if (courseDto == null)
                return false;

            Course oldCourse = _DB.Courses.FirstOrDefault(c => c.Id == id);

            if (oldCourse is null)
                return false;

            oldCourse.Title = courseDto.Title;
            oldCourse.Description = courseDto.Description;
            oldCourse.Price = courseDto.Price;
            oldCourse.Num_Of_Lectures = courseDto.Num_Of_Lectures;
            oldCourse.level = courseDto.level;
            oldCourse.Duration = courseDto.Duration;
            oldCourse.TeacherID = courseDto.TeacherID;


            _DB.SaveChanges();
            return true;
        }

        public Course GetCourseWithDetails(int id)
        {
            return _DB.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Course_Registrations)
                    .ThenInclude(cr => cr.Student)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Materials)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.assignment)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                .FirstOrDefault(c => c.Id == id);
        }

        public List<Course> GetCoursesByTeacher(int teacherId)
        {
            return _DB.Courses
                .Where(c => c.TeacherID == teacherId)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.assignment)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Materials)
                .Include(c => c.Course_Registrations)
                .ToList();
        }

        public List<Course> GetAllCoursesWithDetails()
        {
            return _DB.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lectures)
                .Include(c => c.Course_Registrations)
                .ToList();
        }
    }
}
