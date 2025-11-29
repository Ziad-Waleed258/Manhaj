using Manhaj.Models;

namespace Manhaj.IRepositories
{
    public interface ILectureRepository
    {
        Lecture GetById(int id);
        bool Create(Lecture lecture);
        bool Update(Lecture lecture);
        bool Delete(int id);


        // New methods for refactoring
        Lecture GetLectureWithDetails(int id);
    }
}
