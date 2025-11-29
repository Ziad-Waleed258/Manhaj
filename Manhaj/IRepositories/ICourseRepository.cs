using Manhaj.DTOs;
using Manhaj.Models;
using Microsoft.AspNetCore.Mvc;

namespace Manhaj.IRepositories
{
    public interface ICourseRepository
    {
        public List<CourseVM> GetAll();
        public CourseVM GetById(int id);
        public CourseVM GetByName(string name);

        public bool Update(CourseVM courseDto, int id);

        public bool Delete(int id);
        public void Create(CourseVM courseDto);




        // New methods for refactoring
        public Course GetCourseWithDetails(int id);
        public List<Course> GetCoursesByTeacher(int teacherId);
        public List<Course> GetAllCoursesWithDetails();
    }
}
