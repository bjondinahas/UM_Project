using UM_Project.Models;
namespace UM_Project.Services.Interfaces
{
    public interface IGradeService
    {
        Task<Grade?> GetGradeByIdAsync(int id);
        Task<IEnumerable<Grade>> GetGradesByStudentIdAsync(int studentId);
        Task<IEnumerable<Grade>> GetGradesByCourseIdAsync(int courseId);
        Task<Grade> AssignGradeAsync(int studentId, int courseId, int value);
        Task<Grade> UpdateGradeAsync(int gradeId, int newValue);
        Task<bool> DeleteGradeAsync(int gradeId);
        Task<double> GetStudentAverageAsync(int studentId);
        Task<bool> GradeExistsAsync(int studentId, int courseId);
        string GetLetterGrade(int value);
        string GetGradeColor(int value);
    }
}
