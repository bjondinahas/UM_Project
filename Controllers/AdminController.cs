using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingsService _settings;

        public AdminController(ApplicationDbContext context, ISystemSettingsService settings)
        {
            _context = context;
            _settings = settings;
        }

        public async Task<IActionResult> Dashboard()
        {
            var academic = await _settings.GetAcademicSettingsAsync();
            ViewBag.GradePassingMin = academic.GradePassingMinimum;
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalProfessors = await _context.Professors.CountAsync();
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
            ViewBag.TotalGrades = await _context.Grades.CountAsync();
            ViewBag.TotalSchedules = await _context.Schedules.CountAsync();
            ViewBag.TotalParents = await _context.ParentGuardians.CountAsync();
            ViewBag.RoleCount = RoleNames.All.Length;
            ViewBag.AverageGrade = await _context.Grades.AnyAsync() ? Math.Round(await _context.Grades.AverageAsync(g => g.Value), 2) : 0;

            ViewBag.PassingCount = await _context.Grades.Where(g => g.Value >= academic.GradePassingMinimum).Select(g => g.StudentId).Distinct().CountAsync();
            ViewBag.FailingCount = await _context.Grades.Where(g => g.Value < academic.GradePassingMinimum).Select(g => g.StudentId).Distinct().CountAsync();

            var totalGrades = await _context.Grades.CountAsync();
            ViewBag.PassRate = totalGrades > 0
                ? Math.Round(100.0 * await _context.Grades.CountAsync(g => g.Value >= academic.GradePassingMinimum) / totalGrades, 1)
                : 0;

            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            ViewBag.AbsencesThisMonth = await _context.AttendanceRecords
                .CountAsync(a => !a.IsPresent && a.AttendanceDate >= monthStart);
            ViewBag.AttendanceRecordsTotal = await _context.AttendanceRecords.CountAsync();

            var weekAgo = DateTime.UtcNow.AddDays(-7);
            ViewBag.NewEnrollments7d = await _context.Enrollments.CountAsync(e => e.EnrollmentDate >= weekAgo);

            var today = DateTime.UtcNow.Date;
            ViewBag.LoginsToday = await _context.AuthAuditLogs.CountAsync(l =>
                l.Success && l.EventType == AuthEventTypes.LoginSuccess && l.CreatedAtUtc >= today);
            ViewBag.LoginsWeek = await _context.AuthAuditLogs.CountAsync(l =>
                l.Success && l.EventType == AuthEventTypes.LoginSuccess && l.CreatedAtUtc >= weekAgo);

            ViewBag.DeptLabels = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => d.DepartmentName)
                .ToListAsync();
            ViewBag.DeptStudentCounts = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => d.Students.Count)
                .ToListAsync();

            var gradeMin = academic.GradeMinimum;
            var gradeMax = academic.GradeMaximum;
            ViewBag.GradeLabels = Enumerable.Range(gradeMin, gradeMax - gradeMin + 1).Reverse().Select(v => v.ToString()).ToList();
            var gradeCounts = new List<int>();
            for (var v = gradeMax; v >= gradeMin; v--)
                gradeCounts.Add(await _context.Grades.CountAsync(g => g.Value == v));
            ViewBag.GradeCounts = gradeCounts;

            var loginDays = Enumerable.Range(0, 7).Select(i => DateTime.UtcNow.Date.AddDays(-6 + i)).ToList();
            ViewBag.LoginChartLabels = loginDays.Select(d => d.ToString("ddd")).ToList();
            var loginData = new List<int>();
            foreach (var day in loginDays)
            {
                var next = day.AddDays(1);
                loginData.Add(await _context.AuthAuditLogs.CountAsync(l =>
                    l.Success && l.EventType == AuthEventTypes.LoginSuccess && l.CreatedAtUtc >= day && l.CreatedAtUtc < next));
            }
            ViewBag.LoginChartData = loginData;

            ViewBag.RecentActivity = await _context.AdminActivityLogs.AsNoTracking()
                .OrderByDescending(l => l.CreatedAtUtc)
                .Take(6)
                .ToListAsync();

            ViewBag.GradeMin = gradeMin;
            ViewBag.GradeMax = gradeMax;
            ViewBag.GradePassingMin = academic.GradePassingMinimum;

            return View();
        }

        public async Task<IActionResult> AuthLogs(string? eventType, bool? success, int page = 1)
        {
            const int pageSize = 50;
            IQueryable<AuthAuditLog> query = _context.AuthAuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(eventType))
                query = query.Where(l => l.EventType == eventType);

            if (success.HasValue)
                query = query.Where(l => l.Success == success.Value);

            query = query.OrderByDescending(l => l.CreatedAtUtc);

            var total = await query.CountAsync();
            var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.EventTypes = await _context.AuthAuditLogs
                .Select(l => l.EventType)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();
            ViewBag.CurrentEventType = eventType;
            ViewBag.CurrentSuccess = success;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.TotalLogs = total;

            return View(logs);
        }

        public async Task<IActionResult> ActivityLogs(string? entityType, string? action, int page = 1)
        {
            const int pageSize = 50;
            IQueryable<AdminActivityLog> query = _context.AdminActivityLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(entityType))
                query = query.Where(l => l.EntityType == entityType);
            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(l => l.Action == action);

            query = query.OrderByDescending(l => l.CreatedAtUtc);

            var total = await query.CountAsync();
            var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.EntityTypes = await _context.AdminActivityLogs.Select(l => l.EntityType).Distinct().OrderBy(e => e).ToListAsync();
            ViewBag.Actions = await _context.AdminActivityLogs.Select(l => l.Action).Distinct().OrderBy(a => a).ToListAsync();
            ViewBag.CurrentEntityType = entityType;
            ViewBag.CurrentAction = action;
            ViewBag.Page = page;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            ViewBag.TotalLogs = total;

            return View(logs);
        }
    }
}
