using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StudentController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Dashboard()
        {
            var userEmail = User.Identity?.Name;
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userEmail);
            if (student == null) return View("NoProfile");
            
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Department)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Professor)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Schedules)  
                .Where(e => e.StudentId == student.StudentId)
                .ToListAsync();

            var grades = await _context.Grades
                .Where(g => g.StudentId == student.StudentId)
                .ToDictionaryAsync(g => g.CourseId, g => g);

            ViewBag.StudentName = student.FullName;
            ViewBag.StudentNumber = student.StudentNumber;
            ViewBag.Grades = grades;
            return View(enrollments);
        }

        public async Task<IActionResult> MyGrades()
        {
            var userEmail = User.Identity?.Name;
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userEmail);
            if (student == null) return View("NoProfile");
            var grades = await _context.Grades
                .Include(g => g.Course)
                    .ThenInclude(c => c.Department)
                .Where(g => g.StudentId == student.StudentId)
                .ToListAsync();
            ViewBag.StudentName = student.FullName;
            ViewBag.StudentNumber = student.StudentNumber;
            return View(grades);
        }

        public async Task<IActionResult> Schedules()
        {
            var userEmail = User.Identity?.Name;
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userEmail);
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
