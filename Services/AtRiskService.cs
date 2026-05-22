using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class AtRiskService : IAtRiskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingsService _settings;

        public AtRiskService(ApplicationDbContext context, ISystemSettingsService settings)
        {
            _context = context;
            _settings = settings;
        }

        public async Task<List<AtRiskStudentViewModel>> GetAtRiskStudentsAsync()
        {
            var academic = await _settings.GetAcademicSettingsAsync();
            var absenceDays = await _settings.GetIntAsync(SystemSettingKeys.AtRiskAbsenceDays, 30);
            var absenceThreshold = await _settings.GetIntAsync(SystemSettingKeys.AtRiskAbsenceCount, 5);
            var since = DateTime.UtcNow.Date.AddDays(-absenceDays);

            var students = await _context.Students.Include(s => s.Department).ToListAsync();
            var grades = await _context.Grades.ToListAsync();
            var absences = await _context.AttendanceRecords
                .Where(a => !a.IsPresent && a.AttendanceDate >= since)
                .GroupBy(a => a.StudentId)
                .Select(g => new { StudentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StudentId, x => x.Count);

            var notes = await _context.InterventionNotes
                .OrderByDescending(n => n.CreatedAtUtc)
                .ToListAsync();

            var result = new List<AtRiskStudentViewModel>();

            foreach (var student in students)
            {
                var studentGrades = grades.Where(g => g.StudentId == student.StudentId).ToList();
                if (!studentGrades.Any()) continue;

                var avg = studentGrades.Average(g => g.Value);
                var lowCount = studentGrades.Count(g => g.Value < academic.GradePassingMinimum);
                absences.TryGetValue(student.StudentId, out var absenceCount);

                var reasons = new List<string>();
                if (avg < academic.GradePassingMinimum)
                    reasons.Add($"Nota mesatare {avg:0.0} — nën kalimin (min. {academic.GradePassingMinimum})");
                if (lowCount >= 2)
                    reasons.Add($"{lowCount} lëndë me notë nën kalimin");
                if (absenceCount >= absenceThreshold)
                    reasons.Add($"{absenceCount} mungesa në {absenceDays} ditët e fundit");

                if (!reasons.Any()) continue;

                result.Add(new AtRiskStudentViewModel
                {
                    Student = student,
                    AverageGrade = Math.Round(avg, 2),
                    AbsencesLast30Days = absenceCount,
                    LowGradeCount = lowCount,
                    Reasons = reasons,
                    Notes = notes.Where(n => n.StudentId == student.StudentId).Take(5).ToList()
                });
            }

            return result.OrderByDescending(r => r.AbsencesLast30Days).ThenBy(r => r.AverageGrade).ToList();
        }
    }
}
