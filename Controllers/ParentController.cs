using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.Parent)]
    public class ParentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISystemSettingsService _settings;

        public ParentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ISystemSettingsService settings)
        {
            _context = context;
            _userManager = userManager;
            _settings = settings;
        }

        private async Task<List<ParentGuardian>> GetLinkedChildrenAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new List<ParentGuardian>();

            return await _context.ParentGuardians
                .Include(p => p.Student)
                .ThenInclude(s => s!.Department)
                .Where(p => p.UserId == user.Id)
                .OrderBy(p => p.Student!.FullName)
                .ToListAsync();
        }

        private async Task<ParentGuardian?> GetSelectedChildAsync(int? studentId)
        {
            var children = await GetLinkedChildrenAsync();
            if (!children.Any()) return null;
            if (studentId > 0)
                return children.FirstOrDefault(c => c.StudentId == studentId) ?? children.First();
            return children.First();
        }

        public async Task<IActionResult> Dashboard(int? studentId)
        {
            var children = await GetLinkedChildrenAsync();
            if (!children.Any()) return View("NoProfile");

            var parent = await GetSelectedChildAsync(studentId);
            if (parent == null) return View("NoProfile");

            var grades = await _context.Grades
                .Where(g => g.StudentId == parent.StudentId)
                .ToListAsync();

            var academic = await _settings.GetAcademicSettingsAsync();
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var absencesThisMonth = await _context.AttendanceRecords
                .Where(a => a.StudentId == parent.StudentId && !a.IsPresent && a.AttendanceDate >= monthStart)
                .CountAsync();

            ViewBag.Children = children;
            ViewBag.SelectedStudentId = parent.StudentId;
            ViewBag.StudentName = parent.Student!.FullName;
            ViewBag.StudentNumber = parent.Student.StudentNumber;
            ViewBag.Department = parent.Student.Department?.DepartmentName ?? "—";
            ViewBag.GradeCount = grades.Count;
            ViewBag.EnrollmentCount = await _context.Enrollments.CountAsync(e => e.StudentId == parent.StudentId);
            ViewBag.AverageGrade = grades.Any() ? Math.Round(grades.Average(g => g.Value), 2) : 0;
            ViewBag.PassedCourses = grades.Count(g => g.Value >= academic.GradePassingMinimum);
            ViewBag.FailedCourses = grades.Count(g => g.Value < academic.GradePassingMinimum);
            ViewBag.AbsencesThisMonth = absencesThisMonth;
            ViewBag.GradeLabels = Enumerable.Range(academic.GradeMinimum, academic.GradeMaximum - academic.GradeMinimum + 1)
                .Reverse().Select(v => v.ToString()).ToList();
            ViewBag.GradeCounts = Enumerable.Range(academic.GradeMinimum, academic.GradeMaximum - academic.GradeMinimum + 1)
                .Reverse().Select(v => grades.Count(g => g.Value == v)).ToList();

            return View(parent);
        }

        public async Task<IActionResult> ChildGrades(int? studentId)
        {
            var parent = await GetSelectedChildAsync(studentId);
            if (parent == null) return View("NoProfile");

            var grades = await _context.Grades
                .Include(g => g.Course)
                .Where(g => g.StudentId == parent.StudentId)
                .ToListAsync();

            ViewBag.Children = await GetLinkedChildrenAsync();
            ViewBag.SelectedStudentId = parent.StudentId;
            ViewBag.StudentName = parent.Student!.FullName;
            ViewBag.StudentNumber = parent.Student.StudentNumber;
            return View(grades);
        }

        public async Task<IActionResult> ChildSchedule(int? studentId)
        {
            var parent = await GetSelectedChildAsync(studentId);
            if (parent == null) return View("NoProfile");

            var courseIds = await _context.Enrollments
                .Where(e => e.StudentId == parent.StudentId)
                .Select(e => e.CourseId)
                .ToListAsync();
            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Where(s => courseIds.Contains(s.CourseId))
                .ToListAsync();

            ViewBag.Children = await GetLinkedChildrenAsync();
            ViewBag.SelectedStudentId = parent.StudentId;
            ViewBag.StudentName = parent.Student!.FullName;
            ViewBag.LessonSlots = await _context.LessonSlots.Where(l => l.IsActive).OrderBy(l => l.SlotNumber).ToListAsync();
            return View(schedules);
        }

        public async Task<IActionResult> ChildAttendance(int? studentId, int? year, int? month)
        {
            var parent = await GetSelectedChildAsync(studentId);
            if (parent == null) return View("NoProfile");

            var y = year ?? DateTime.Today.Year;
            var m = month ?? DateTime.Today.Month;
            if (m < 1) { m = 12; y--; }
            if (m > 12) { m = 1; y++; }

            var academic = await _settings.GetAcademicSettingsAsync();
            var slots = await _context.LessonSlots
                .Where(l => l.IsActive)
                .OrderBy(l => l.SlotNumber)
                .Take(academic.LessonsPerDay)
                .ToListAsync();

            var start = new DateTime(y, m, 1);
            var end = start.AddMonths(1);

            var records = await _context.AttendanceRecords
                .Include(a => a.Course)
                .Include(a => a.LessonSlot)
                .Where(a => a.StudentId == parent.StudentId && a.AttendanceDate >= start && a.AttendanceDate < end)
                .ToListAsync();

            var days = new List<ParentDayAttendance>();
            for (var d = start; d < end; d = d.AddDays(1))
            {
                var dayRecords = records.Where(r => r.AttendanceDate.Date == d.Date).ToList();
                var absent = dayRecords.Where(r => !r.IsPresent).ToList();
                if (!dayRecords.Any()) continue;

                days.Add(new ParentDayAttendance
                {
                    Date = d,
                    TotalSlots = slots.Count,
                    AbsentCount = absent.Count,
                    Details = absent.Select(a => new ParentAbsenceDetail
                    {
                        CourseName = a.Course?.CourseName ?? "—",
                        LessonTitle = a.LessonSlot?.Title ?? $"Lesson {a.LessonSlot?.SlotNumber}",
                        TimeRange = $"{a.LessonSlot?.StartTime}–{a.LessonSlot?.EndTime}"
                    }).ToList()
                });
            }

            var vm = new ParentAttendanceCalendarViewModel
            {
                Student = parent.Student!,
                Year = y,
                Month = m,
                LessonSlots = slots,
                Days = days,
                LinkedChildren = await GetLinkedChildrenAsync(),
                SelectedStudentId = parent.StudentId
            };
            return View(vm);
        }

        public async Task<IActionResult> ChildAssignments(int? studentId)
        {
            var parent = await GetSelectedChildAsync(studentId);
            if (parent == null) return View("NoProfile");

            var courseIds = await _context.Enrollments
                .Where(e => e.StudentId == parent.StudentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var assignments = await _context.CourseAssignments
                .Include(a => a.Course).ThenInclude(c => c!.Professor)
                .Where(a => courseIds.Contains(a.CourseId))
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            ViewBag.Children = await GetLinkedChildrenAsync();
            ViewBag.SelectedStudentId = parent.StudentId;
            ViewBag.StudentName = parent.Student!.FullName;
            ViewBag.Upcoming = assignments.Where(a => a.DueDate >= now).ToList();
            ViewBag.Past = assignments.Where(a => a.DueDate < now).ToList();
            return View(assignments);
        }
    }
}
