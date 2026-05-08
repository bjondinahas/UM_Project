using UM_Project.Models;
namespace UM_Project.Services.Interfaces
{
    public interface IStudentService
    {
        Task<Student?> GetStudentByIdAsync(int id);
        Task<Student?> GetStudentByEmailAsync(string email);
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task<Student> CreateStudentAsync(Student student);
        Task<Student> UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int id);
        Task<IEnumerable<Course>> GetStudentCoursesAsync(int studentId);
        Task<bool> EnrollStudentInCourseAsync(int studentId, int courseId);
        Task<bool> WithdrawStudentFromCourseAsync(int studentId, int courseId);
        Task<bool> IsEnrolledAsync(int studentId, int courseId);
    }
}
