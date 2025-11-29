using Manhaj.IRepositories;
using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Repositories
{
    public class LectureRepository : ILectureRepository
    {
        private readonly ManhajDbContext _context;

        public LectureRepository(ManhajDbContext context)
        {
            _context = context;
        }

        public bool Create(Lecture lecture)
        {
            try
            {
                _context.Lectures.Add(lecture);
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
            var lecture = _context.Lectures.Find(id);
            if (lecture == null) return false;

            _context.Lectures.Remove(lecture);
            _context.SaveChanges();
            return true;
        }

        public Lecture GetById(int id)
        {
            return _context.Lectures
                .Include(l => l.Course)
                .Include(l => l.Materials)
                .Include(l => l.assignment)
                .Include(l => l.Quiz)
                .FirstOrDefault(l => l.Id == id);
        }

        public bool Update(Lecture lecture)
        {
            try
            {
                _context.Lectures.Update(lecture);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Lecture GetLectureWithDetails(int id)
        {
            return GetById(id);
        }
    }
}
