using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Resources;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    [Route("[controller]")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingsService _settings;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public StatisticsController(ApplicationDbContext context, ISystemSettingsService settings, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _settings = settings;
            _localizer = localizer;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(
            int? departmentId,
            int? courseId,
            int? professorId,
            int? gradeValue,
            string? status,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? search)
        {
            var filter = new AnalyticsFilterInput
            {
                DepartmentId = departmentId,
                CourseId = courseId,
                ProfessorId = professorId,
                GradeValue = gradeValue,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo?.Date.AddDays(1).AddTicks(-1),
                Search = search?.Trim()
            };

            var academic = await _settings.GetAcademicSettingsAsync();
            var vm = new AnalyticsViewModel
            {
                Filter = filter,
                Departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync(),
                Courses = await _context.Courses.OrderBy(c => c.CourseName).ToListAsync(),
                Professors = await _context.Professors.OrderBy(p => p.FullName).ToListAsync(),
                GradeMinimum = academic.GradeMinimum,
                GradeMaximum = academic.GradeMaximum,
                GradePassingMinimum = academic.GradePassingMinimum
            };

            var gradesQuery = _context.Grades
                .Include(g => g.Student).ThenInclude(s => s.Department)
                .Include(g => g.Course).ThenInclude(c => c.Department)
                .Include(g => g.Course).ThenInclude(c => c.Professor)
                .AsQueryable();

            if (filter.DepartmentId.HasValue)
                gradesQuery = gradesQuery.Where(g => g.Course.DepartmentId == filter.DepartmentId.Value);
            if (filter.CourseId.HasValue)
                gradesQuery = gradesQuery.Where(g => g.CourseId == filter.CourseId.Value);
            if (filter.ProfessorId.HasValue)
                gradesQuery = gradesQuery.Where(g => g.Course.ProfessorId == filter.ProfessorId.Value);
            if (filter.GradeValue.HasValue)
                gradesQuery = gradesQuery.Where(g => g.Value == filter.GradeValue.Value);
            if (filter.DateFrom.HasValue)
                gradesQuery = gradesQuery.Where(g => g.DateRecorded >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                gradesQuery = gradesQuery.Where(g => g.DateRecorded <= filter.DateTo.Value);
            if (filter.Status == "pass")
                gradesQuery = gradesQuery.Where(g => g.Value >= academic.GradePassingMinimum);
            else if (filter.Status == "fail")
                gradesQuery = gradesQuery.Where(g => g.Value < academic.GradePassingMinimum);

            var grades = await gradesQuery.ToListAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var q = filter.Search.ToLowerInvariant();
                grades = grades.Where(g =>
                    (g.Student?.FullName?.ToLowerInvariant().Contains(q) ?? false) ||
                    (g.Student?.StudentNumber?.ToLowerInvariant().Contains(q) ?? false) ||
                    (g.Course?.CourseName?.ToLowerInvariant().Contains(q) ?? false) ||
                    (g.Course?.CourseCode?.ToLowerInvariant().Contains(q) ?? false)
                ).ToList();
            }

            vm.TotalGrades = grades.Count;
            vm.AverageGrade = grades.Count > 0 ? Math.Round(grades.Average(g => g.Value), 2) : 0;
            for (var v = academic.GradeMaximum; v >= academic.GradeMinimum; v--)
            {
                vm.GradeHistogramLabels.Add(v.ToString());
                vm.GradeHistogram.Add(grades.Count(g => g.Value == v));
            }
            vm.PassingStudents = grades.Where(g => g.Value >= academic.GradePassingMinimum).Select(g => g.StudentId).Distinct().Count();
            vm.FailingStudents = grades.Where(g => g.Value < academic.GradePassingMinimum).Select(g => g.StudentId).Distinct().Count();

            var studentsQuery = _context.Students.Include(s => s.Department).AsQueryable();
            if (filter.DepartmentId.HasValue)
                studentsQuery = studentsQuery.Where(s => s.DepartmentId == filter.DepartmentId.Value);

            var students = await studentsQuery.ToListAsync();
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var q = filter.Search.ToLowerInvariant();
                students = students.Where(s =>
                    s.FullName.ToLowerInvariant().Contains(q) ||
                    s.StudentNumber.ToLowerInvariant().Contains(q) ||
                    s.Email.ToLowerInvariant().Contains(q)
                ).ToList();
            }

            vm.TotalStudents = students.Count;
            vm.TotalProfessors = filter.ProfessorId.HasValue ? 1 : await _context.Professors.CountAsync();
            vm.TotalCourses = filter.CourseId.HasValue ? 1 :
                (filter.DepartmentId.HasValue
                    ? await _context.Courses.CountAsync(c => c.DepartmentId == filter.DepartmentId)
                    : await _context.Courses.CountAsync());

            var enrollQuery = _context.Enrollments.AsQueryable();
            if (filter.CourseId.HasValue)
                enrollQuery = enrollQuery.Where(e => e.CourseId == filter.CourseId.Value);
            else if (filter.DepartmentId.HasValue)
            {
                var courseIds = await _context.Courses
                    .Where(c => c.DepartmentId == filter.DepartmentId)
                    .Select(c => c.CourseId)
                    .ToListAsync();
                enrollQuery = enrollQuery.Where(e => courseIds.Contains(e.CourseId));
            }
            vm.TotalEnrollments = await enrollQuery.CountAsync();

            var unknown = _localizer["Common_Unknown"].Value;
            var noData = _localizer["Common_NoData"].Value;

            vm.DeptLabels = students
                .GroupBy(s => s.Department?.DepartmentName ?? unknown)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();
            vm.DeptCounts = students
                .GroupBy(s => s.Department?.DepartmentName ?? unknown)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Count())
                .ToList();
            if (!vm.DeptLabels.Any()) { vm.DeptLabels.Add(noData); vm.DeptCounts.Add(0); }

            var courseGroups = grades
                .GroupBy(g => g.Course?.CourseName ?? unknown)
                .OrderByDescending(g => g.Count())
                .Take(8)
                .ToList();
            vm.CourseLabels = courseGroups.Select(g => Truncate(g.Key, 24)).ToList();
            vm.CourseCounts = courseGroups.Select(g => g.Count()).ToList();
            if (!vm.CourseLabels.Any()) { vm.CourseLabels.Add(noData); vm.CourseCounts.Add(0); }

            var statusPass = _localizer["Status_Pass"].Value;
            var statusFail = _localizer["Status_Fail"].Value;

            vm.GradeRows = grades
                .OrderByDescending(g => g.DateRecorded)
                .Take(500)
                .Select(g => new AnalyticsGradeRow
                {
                    StudentName = g.Student?.FullName ?? "—",
                    StudentNumber = g.Student?.StudentNumber ?? "—",
                    CourseName = g.Course?.CourseName ?? "—",
                    DepartmentName = g.Course?.Department?.DepartmentName ?? "—",
                    ProfessorName = g.Course?.Professor?.FullName ?? "—",
                    Value = g.Value,
                    Status = g.Value >= academic.GradePassingMinimum ? statusPass : statusFail,
                    DateRecorded = g.DateRecorded
                })
                .ToList();

            var studentGradeAvgs = grades.GroupBy(g => g.StudentId)
                .ToDictionary(g => g.Key, g => g.Average(x => x.Value));

            var enrollCounts = await _context.Enrollments
                .GroupBy(e => e.StudentId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            vm.StudentRows = students
                .OrderBy(s => s.FullName)
                .Select(s => new AnalyticsStudentRow
                {
                    FullName = s.FullName,
                    StudentNumber = s.StudentNumber,
                    Email = s.Email,
                    DepartmentName = s.Department?.DepartmentName ?? "—",
                    EnrollmentCount = enrollCounts.GetValueOrDefault(s.StudentId),
                    AverageGrade = studentGradeAvgs.TryGetValue(s.StudentId, out var avg)
                        ? Math.Round(avg, 2) : null
                })
                .ToList();

            return View(vm);
        }

        private static string Truncate(string text, int max) =>
            text.Length <= max ? text : text[..max] + "…";
    }
}
