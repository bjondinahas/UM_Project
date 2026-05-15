using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;

namespace UM_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalProfessors = await _context.Professors.CountAsync();
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
            ViewBag.TotalGrades = await _context.Grades.CountAsync();
            ViewBag.TotalSchedules = await _context.Schedules.CountAsync();  // Shto këtë rresht
            ViewBag.AverageGrade = await _context.Grades.AnyAsync() ? Math.Round(await _context.Grades.AverageAsync(g => g.Value), 2) : 0;

            ViewBag.Grade10 = await _context.Grades.CountAsync(g => g.Value == 10);
            ViewBag.Grade9 = await _context.Grades.CountAsync(g => g.Value == 9);
            ViewBag.Grade8 = await _context.Grades.CountAsync(g => g.Value == 8);
            ViewBag.Grade7 = await _context.Grades.CountAsync(g => g.Value == 7);
            ViewBag.Grade6 = await _context.Grades.CountAsync(g => g.Value == 6);
            ViewBag.Grade5 = await _context.Grades.CountAsync(g => g.Value == 5);

            ViewBag.PassingCount = await _context.Grades.Where(g => g.Value >= 6).Select(g => g.StudentId).Distinct().CountAsync();
            ViewBag.FailingCount = await _context.Grades.Where(g => g.Value == 5).Select(g => g.StudentId).Distinct().CountAsync();

            return View();
        }
    }
}
