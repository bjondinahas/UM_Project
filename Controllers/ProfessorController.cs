using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProfessorController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Dashboard()
        {
            var userEmail = User.Identity?.Name;
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.Email == userEmail);
            if (professor == null) return View("NoProfile");
            var courses = await _context.Courses
                .Include(c => c.Department)
                .Where(c => c.ProfessorId == professor.ProfessorId)
                .ToListAsync();
            ViewBag.ProfessorName = professor.FullName;
            ViewBag.ProfessorEmail = professor.Email;
            ViewBag.ProfessorDepartment = professor.Department?.DepartmentName ?? "N/A";
            return View(courses);
        }

        public async Task<IActionResult> CourseStudents(int id)
        {
            var course = await _context.Courses.Include(c => c.Department).FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null) return NotFound();
            var students = await _context.Enrollments.Include(e => e.Student).Where(e => e.CourseId == id).Select(e => e.Student).ToListAsync();
            var grades = await _context.Grades.Where(g => g.CourseId == id).ToDictionaryAsync(g => g.StudentId, g => g);
            ViewBag.Course = course;
            ViewBag.Grades = grades;
            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> SetGrade(int studentId, int courseId, int gradeValue)
        {
            var grade = await _context.Grades.FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);
            if (grade != null)
            {
                grade.Value = gradeValue;
                grade.DateRecorded = DateTime.Now;
                _context.Update(grade);
            }
            else
            {
                _context.Add(new Grade { StudentId = studentId, CourseId = courseId, Value = gradeValue, DateRecorded = DateTime.Now });
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Grade saved!";
            return RedirectToAction(nameof(CourseStudents), new { id = courseId });
        }

        public async Task<IActionResult> Schedules()
        {
            var userEmail = User.Identity?.Name;
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.Email == userEmail);
            if (professor == null) return View("NoProfile");
            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Where(s => s.Course.ProfessorId == professor.ProfessorId)
                .ToListAsync();
            return View(schedules);
        }
    }
}
