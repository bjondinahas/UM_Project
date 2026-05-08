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
            ViewBag.AverageGrade = await _context.Grades.AnyAsync() ? Math.Round(await _context.Grades.AverageAsync(g => g.Value), 2) : 0;

           
            ViewBag.Grade10 = await _context.Grades.CountAsync(g => g.Value == 10);
            ViewBag.Grade9 = await _context.Grades.CountAsync(g => g.Value == 9);
            ViewBag.Grade8 = await _context.Grades.CountAsync(g => g.Value == 8);
            ViewBag.Grade7 = await _context.Grades.CountAsync(g => g.Value == 7);
            ViewBag.Grade6 = await _context.Grades.CountAsync(g => g.Value == 6);
            ViewBag.Grade5 = await _context.Grades.CountAsync(g => g.Value == 5);

          
            var passingStudentsCount = await _context.Grades.Where(g => g.Value >= 6).Select(g => g.StudentId).Distinct().CountAsync();
            var failingStudentsCount = await _context.Grades.Where(g => g.Value == 5).Select(g => g.StudentId).Distinct().CountAsync();
            var totalStudentsWithGrades = passingStudentsCount + failingStudentsCount;

            ViewBag.PassingCount = passingStudentsCount;
            ViewBag.FailingCount = failingStudentsCount;
            ViewBag.PassingPercentage = totalStudentsWithGrades > 0 ? Math.Round((double)passingStudentsCount / totalStudentsWithGrades * 100, 1) : 0;
            ViewBag.FailingPercentage = totalStudentsWithGrades > 0 ? Math.Round((double)failingStudentsCount / totalStudentsWithGrades * 100, 1) : 0;

            return View();
        }
    }
}
