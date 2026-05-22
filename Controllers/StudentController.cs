using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.Student)]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StudentController(ApplicationDbContext context) => _context = context;

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
            ViewBag.PassedCourses = gradeList.Count(g => g.Value >= 6);
            ViewBag.FailedCourses = gradeList.Count(g => g.Value == 5);
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
