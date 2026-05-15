using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ProfessorController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userEmail = User.Identity?.Name;
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.Email == userEmail);
            if (professor == null) return View("NoProfile");
            var courses = await _context.Courses.Include(c => c.Department).Where(c => c.ProfessorId == professor.ProfessorId).ToListAsync();
            ViewBag.ProfessorName = professor.FullName;
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
            // Validate grade value
            if (gradeValue < 5 || gradeValue > 10)
            {
                TempData["Error"] = "Grade must be between 5 and 10!";
                return RedirectToAction(nameof(CourseStudents), new { id = courseId });
            }
            
            var existingGrade = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);
            
            if (existingGrade != null)
            {
                TempData["Error"] = "You cannot change an already assigned grade. Please contact the administrator if correction is needed.";
                return RedirectToAction(nameof(CourseStudents), new { id = courseId });
            }
            
            var newGrade = new Grade
            {
                StudentId = studentId,
                CourseId = courseId,
                Value = gradeValue,
                DateRecorded = DateTime.Now
            };
            _context.Add(newGrade);
            await _context.SaveChangesAsync();
            
            // Send email notification to student (optional)
            var student = await _context.Students.FindAsync(studentId);
            var course = await _context.Courses.FindAsync(courseId);
            if (student != null && course != null && !string.IsNullOrEmpty(student.Email))
            {
                await _emailService.SendGradeNotificationAsync(student.Email, student.FullName, course.CourseName, gradeValue);
            }
            
            TempData["Success"] = "Grade saved successfully! Email notification sent.";
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
