using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EnrollmentsController(ApplicationDbContext context) => _context = context;

        private async Task<bool> HasStudentScheduleConflict(int studentId, int courseId, int? excludeEnrollmentId = null)
        {
            var newCourseSchedules = await _context.Schedules.Where(s => s.CourseId == courseId).ToListAsync();
            if (!newCourseSchedules.Any()) return false; // Nëse lënda nuk ka asnjë orar, nuk ka konflikt

            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.EnrollmentId != excludeEnrollmentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var enrolledSchedules = await _context.Schedules
                .Where(s => enrolledCourseIds.Contains(s.CourseId))
                .ToListAsync();

            foreach (var newSched in newCourseSchedules)
            {
                foreach (var enrolledSched in enrolledSchedules)
                {
                    if (enrolledSched.Day == newSched.Day && enrolledSched.Time == newSched.Time)
                    {
                        return true; 
                    }
                }
            }
            return false;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = await _context.Enrollments.Include(e => e.Student).Include(e => e.Course).ThenInclude(c => c.Department).ToListAsync();
            return View(enrollments);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Students = await _context.Students.ToListAsync();
            ViewBag.Courses = await _context.Courses.Include(c => c.Department).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int studentId, int courseId)
        {
            var existing = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            if (existing != null)
            {
                TempData["Error"] = "Student is already enrolled in this course!";
                ViewBag.Students = await _context.Students.ToListAsync();
                ViewBag.Courses = await _context.Courses.Include(c => c.Department).ToListAsync();
                return View();
            }

            if (await HasStudentScheduleConflict(studentId, courseId))
            {
                TempData["Error"] = "Cannot enroll: the student has another course at the same day/time as this course's schedule.";
                ViewBag.Students = await _context.Students.ToListAsync();
                ViewBag.Courses = await _context.Courses.Include(c => c.Department).ToListAsync();
                return View();
            }

            var enrollment = new Enrollment { StudentId = studentId, CourseId = courseId, EnrollmentDate = DateTime.Now };
            _context.Add(enrollment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Student enrolled successfully!";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Enrollment deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
