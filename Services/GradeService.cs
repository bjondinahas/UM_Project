using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
namespace UM_Project.Services
{
    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GradeService> _logger;
        public GradeService(ApplicationDbContext context, ILogger<GradeService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Grade?> GetGradeByIdAsync(int id) => await _context.Grades.Include(g => g.Student).Include(g => g.Course).FirstOrDefaultAsync(g => g.GradeId == id);
        public async Task<IEnumerable<Grade>> GetGradesByStudentIdAsync(int studentId) => await _context.Grades.Include(g => g.Course).Where(g => g.StudentId == studentId).ToListAsync();
        public async Task<IEnumerable<Grade>> GetGradesByCourseIdAsync(int courseId) => await _context.Grades.Include(g => g.Student).Where(g => g.CourseId == courseId).ToListAsync();
        public async Task<Grade> AssignGradeAsync(int studentId, int courseId, int value)
        {
            if (value < 5 || value > 10) throw new ArgumentException("Grade must be between 5 and 10");
            if (await GradeExistsAsync(studentId, courseId)) throw new InvalidOperationException("Grade already exists");
            var grade = new Grade { StudentId = studentId, CourseId = courseId, Value = value, DateRecorded = DateTime.Now };
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
            return grade;
        }
        public async Task<Grade> UpdateGradeAsync(int gradeId, int newValue)
        {
            if (newValue < 5 || newValue > 10) throw new ArgumentException("Grade must be between 5 and 10");
            var grade = await GetGradeByIdAsync(gradeId);
            if (grade == null) throw new KeyNotFoundException();
            grade.Value = newValue;
            grade.DateRecorded = DateTime.Now;
            _context.Update(grade);
            await _context.SaveChangesAsync();
            return grade;
        }
        public async Task<bool> DeleteGradeAsync(int gradeId)
        {
            var grade = await _context.Grades.FindAsync(gradeId);
            if (grade != null) _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            return grade != null;
        }
        public async Task<double> GetStudentAverageAsync(int studentId)
        {
            var grades = await GetGradesByStudentIdAsync(studentId);
            return grades.Any() ? grades.Average(g => g.Value) : 0;
        }
        public async Task<bool> GradeExistsAsync(int studentId, int courseId) => await _context.Grades.AnyAsync(g => g.StudentId == studentId && g.CourseId == courseId);
        public string GetLetterGrade(int value) => value >= 90 ? "A" : value >= 80 ? "B" : value >= 70 ? "C" : value >= 60 ? "D" : "F";
        public string GetGradeColor(int value) => value >= 70 ? "success" : "danger";
    }
}
