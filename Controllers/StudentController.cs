using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.Student)]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingsService _settings;
        public StudentController(ApplicationDbContext context, ISystemSettingsService settings)
        {
            _context = context;
            _settings = settings;
        }

        private async Task<Student?> GetCurrentStudentAsync()
        {
            var userEmail = User.Identity?.Name;
            return await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Email == userEmail);
        }

        public async Task<IActionResult> Dashboard()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return View("NoProfile");

            var enrollments = await _context.Enrollments
                .Include(e => e.Course).ThenInclude(c => c!.Department)
                .Include(e => e.Course).ThenInclude(c => c!.Professor)
                .Include(e => e.Course).ThenInclude(c => c!.Schedules)
                .Where(e => e.StudentId == student.StudentId)
                .ToListAsync();

            var grades = await _context.Grades
                .Where(g => g.StudentId == student.StudentId)
                .ToDictionaryAsync(g => g.CourseId, g => g);

            var gradeList = grades.Values.ToList();
            ViewBag.StudentName = student.FullName;
            ViewBag.StudentNumber = student.StudentNumber;
            ViewBag.Department = student.Department?.DepartmentName ?? "—";
            ViewBag.Grades = grades;
            ViewBag.AverageGrade = gradeList.Any() ? Math.Round(gradeList.Average(g => g.Value), 2) : 0;
            var academic = await _settings.GetAcademicSettingsAsync();
            ViewBag.GradePassingMin = academic.GradePassingMinimum;
            ViewBag.PassedCourses = gradeList.Count(g => g.Value >= academic.GradePassingMinimum);
            ViewBag.FailedCourses = gradeList.Count(g => g.Value < academic.GradePassingMinimum);
            ViewBag.GradedCount = gradeList.Count;

            return View(enrollments);
        }

        public async Task<IActionResult> MyGrades()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return View("NoProfile");
            var grades = await _context.Grades
                .Include(g => g.Course).ThenInclude(c => c!.Department)
                .Where(g => g.StudentId == student.StudentId)
                .ToListAsync();
            ViewBag.StudentName = student.FullName;
            var academic = await _settings.GetAcademicSettingsAsync();
            ViewBag.GradePassingMin = academic.GradePassingMinimum;
            return View(grades);
        }

        public async Task<IActionResult> Schedules()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return View("NoProfile");
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == student.StudentId)
                .Select(e => e.CourseId)
                .ToListAsync();
            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Where(s => enrolledCourseIds.Contains(s.CourseId))
                .ToListAsync();
            return View(schedules);
        }
    }
}
