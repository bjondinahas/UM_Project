using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
namespace UM_Project.Services
{
    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingsService _settings;
        private readonly ILogger<GradeService> _logger;
        public GradeService(ApplicationDbContext context, ISystemSettingsService settings, ILogger<GradeService> logger)
        {
            _context = context;
            _settings = settings;
            _logger = logger;
        }
        public async Task<Grade?> GetGradeByIdAsync(int id) => await _context.Grades.Include(g => g.Student).Include(g => g.Course).FirstOrDefaultAsync(g => g.GradeId == id);
        public async Task<IEnumerable<Grade>> GetGradesByStudentIdAsync(int studentId) => await _context.Grades.Include(g => g.Course).Where(g => g.StudentId == studentId).ToListAsync();
        public async Task<IEnumerable<Grade>> GetGradesByCourseIdAsync(int courseId) => await _context.Grades.Include(g => g.Student).Where(g => g.CourseId == courseId).ToListAsync();
        public async Task<Grade> AssignGradeAsync(int studentId, int courseId, int value)
        {
            if (!await _settings.IsValidGradeAsync(value))
            {
                var a = await _settings.GetAcademicSettingsAsync();
                throw new ArgumentException($"Grade must be between {a.GradeMinimum} and {a.GradeMaximum}");
            }
            if (await GradeExistsAsync(studentId, courseId)) throw new InvalidOperationException("Grade already exists");
            var grade = new Grade { StudentId = studentId, CourseId = courseId, Value = value, DateRecorded = DateTime.Now };
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
            return grade;
        }
        public async Task<Grade> UpdateGradeAsync(int gradeId, int newValue)
        {
            if (!await _settings.IsValidGradeAsync(newValue))
            {
                var a = await _settings.GetAcademicSettingsAsync();
                throw new ArgumentException($"Grade must be between {a.GradeMinimum} and {a.GradeMaximum}");
            }
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
        public string GetLetterGrade(int value) => value switch { 5 => "A", 4 => "B", 3 => "C", 2 => "D", _ => "F" };
        public string GetGradeColor(int value) => value >= 3 ? "success" : "danger";
    }
}
