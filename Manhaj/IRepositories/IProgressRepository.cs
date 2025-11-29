namespace Manhaj.IRepositories
{
    public interface IProgressRepository
    {
        int GetStudentProgress(int studentId, int courseId);
        bool ToggleProgress(int studentId, int lectureId);
        List<int> GetCompletedLectures(int studentId, int courseId);
    }
}
