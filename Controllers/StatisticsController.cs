using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;

namespace UM_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StatisticsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalProfessors = await _context.Professors.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();
            ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
            ViewBag.TotalGrades = await _context.Grades.CountAsync();
            ViewBag.AverageGrade = await _context.Grades.AnyAsync() ? Math.Round(await _context.Grades.AverageAsync(g => g.Value), 2) : 0;

            var dist = await _context.Grades.GroupBy(g => g.Value).Select(g => new { Grade = g.Key, Count = g.Count() }).ToListAsync();
            ViewBag.GradeDistribution = dist.ToDictionary(d => d.Grade, d => d.Count);
            return View();
        }
    }
}
