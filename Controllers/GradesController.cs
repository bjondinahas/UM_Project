using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminAndProfessor)]
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingsService _settings;
        private readonly IAdminAuditService _audit;

        public GradesController(ApplicationDbContext context, ISystemSettingsService settings, IAdminAuditService audit)
        {
            _context = context;
            _settings = settings;
            _audit = audit;
        }

        private async Task SetGradeRangeViewBagAsync()
        {
            var a = await _settings.GetAcademicSettingsAsync();
            ViewBag.GradeMin = a.GradeMinimum;
            ViewBag.GradeMax = a.GradeMaximum;
            ViewBag.GradePassingMin = a.GradePassingMinimum;
        }

        public async Task<IActionResult> Index()
        {
            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Course)
                .ThenInclude(c => c.Department)
                .ToListAsync();
            return View(grades);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Students = await _context.Students.ToListAsync();
            ViewBag.Courses = await _context.Courses.Include(c => c.Department).ToListAsync();
            await SetGradeRangeViewBagAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int studentId, int courseId, int value)
        {
            if (!await _settings.IsValidGradeAsync(value))
            {
                var a = await _settings.GetAcademicSettingsAsync();
                TempData["Error"] = $"Grade must be between {a.GradeMinimum} and {a.GradeMaximum}!";
                ViewBag.Students = await _context.Students.ToListAsync();
                ViewBag.Courses = await _context.Courses.Include(c => c.Department).ToListAsync();
                await SetGradeRangeViewBagAsync();
                return View();
            }
            var existing = await _context.Grades.FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);
            if (existing != null)
            {
                TempData["Error"] = "Grade already exists! Use Edit.";
                ViewBag.Students = await _context.Students.ToListAsync();
                ViewBag.Courses = await _context.Courses.Include(c => c.Department).ToListAsync();
                return View();
            }
            var grade = new Grade { StudentId = studentId, CourseId = courseId, Value = value, DateRecorded = DateTime.Now };
            _context.Add(grade);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Grade {value} assigned!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();
            ViewBag.Students = await _context.Students.ToListAsync();
            ViewBag.Courses = await _context.Courses.Include(c => c.Department).ToListAsync();
            await SetGradeRangeViewBagAsync();
            return View(grade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int studentId, int courseId, int value)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null) return NotFound();
            var oldValue = grade.Value;
            if (!await _settings.IsValidGradeAsync(value))
            {
                var a = await _settings.GetAcademicSettingsAsync();
                TempData["Error"] = $"Grade must be between {a.GradeMinimum} and {a.GradeMaximum}!";
                ViewBag.Students = await _context.Students.ToListAsync();
                ViewBag.Courses = await _context.Courses.Include(c => c.Department).ToListAsync();
                await SetGradeRangeViewBagAsync();
                return View(grade);
            }
            grade.StudentId = studentId;
            grade.CourseId = courseId;
            grade.Value = value;
            grade.DateRecorded = DateTime.Now;
            _context.Update(grade);
            await _context.SaveChangesAsync();
            if (User.IsInRole(RoleNames.SuperAdmin) || User.IsInRole(RoleNames.Admin))
            {
                await _audit.LogAsync(HttpContext, AdminActions.GradeOverride, AuditEntityTypes.Grade,
                    id.ToString(), $"Student={studentId}, Course={courseId}, {oldValue}→{value}");
            }
            TempData["Success"] = "Grade updated!";
            return RedirectToAction(nameof(Index));
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade != null) _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Grade deleted!";
            return RedirectToAction(nameof(Index));
        }
    }
}
