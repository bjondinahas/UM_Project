using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
namespace UM_Project.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentService> _logger;
        public StudentService(ApplicationDbContext context, ILogger<StudentService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Student?> GetStudentByIdAsync(int id) => await _context.Students.Include(s => s.Department).FirstOrDefaultAsync(s => s.StudentId == id);
        public async Task<Student?> GetStudentByEmailAsync(string email) => await _context.Students.Include(s => s.Department).FirstOrDefaultAsync(s => s.Email == email);
        public async Task<IEnumerable<Student>> GetAllStudentsAsync() => await _context.Students.Include(s => s.Department).ToListAsync();
        public async Task<Student> CreateStudentAsync(Student student) { _context.Add(student); await _context.SaveChangesAsync(); return student; }
        public async Task<Student> UpdateStudentAsync(Student student) { _context.Update(student); await _context.SaveChangesAsync(); return student; }
        public async Task<bool> DeleteStudentAsync(int id) { var s = await _context.Students.FindAsync(id); if (s != null) _context.Students.Remove(s); await _context.SaveChangesAsync(); return s != null; }
        public async Task<IEnumerable<Course>> GetStudentCoursesAsync(int studentId) => await _context.Enrollments.Where(e => e.StudentId == studentId).Select(e => e.Course).ToListAsync();
        public async Task<bool> EnrollStudentInCourseAsync(int studentId, int courseId) { if (await IsEnrolledAsync(studentId, courseId)) return false; _context.Enrollments.Add(new Enrollment { StudentId = studentId, CourseId = courseId, EnrollmentDate = DateTime.Now }); await _context.SaveChangesAsync(); return true; }
        public async Task<bool> WithdrawStudentFromCourseAsync(int studentId, int courseId) { var e = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId); if (e != null) _context.Enrollments.Remove(e); await _context.SaveChangesAsync(); return e != null; }
        public async Task<bool> IsEnrolledAsync(int studentId, int courseId) => await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }
}
