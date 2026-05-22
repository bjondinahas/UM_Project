using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminAndProfessor)]
    public class AssignmentsController : Controller
    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context, IUiText ui)
        {
            _ui = ui;

            _context = context;
        }

        private async Task<Professor?> GetCurrentProfessorAsync()
        {
            var email = User.Identity?.Name;
            return await _context.Professors.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<IActionResult> CourseAssignments(int courseId)
        {
            var course = await _context.Courses.Include(c => c.Department).FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null) return NotFound();

            if (User.IsInRole(RoleNames.Professor) && !User.IsInRole(RoleNames.Admin) && !User.IsInRole(RoleNames.SuperAdmin))
            {
                var prof = await GetCurrentProfessorAsync();
                if (prof == null || course.ProfessorId != prof.ProfessorId) return Forbid();
            }

            var assignments = await _context.CourseAssignments
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            ViewBag.Course = course;
            return View(assignments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId, string title, string? description, DateTime dueDate, decimal weightPercent, int? maxPoints)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            if (User.IsInRole(RoleNames.Professor))
            {
                var prof = await GetCurrentProfessorAsync();
                if (prof == null || course.ProfessorId != prof.ProfessorId) return Forbid();
            }

            if (string.IsNullOrWhiteSpace(title) || weightPercent < 0 || weightPercent > 100)
            {
                TempData["Error"] = _ui["Flash_AssignmentInvalid"];
                return RedirectToAction(nameof(CourseAssignments), new { courseId });
            }

            _context.CourseAssignments.Add(new CourseAssignment
            {
                CourseId = courseId,
                Title = title.Trim(),
                Description = description,
                DueDate = dueDate,
                WeightPercent = weightPercent,
                MaxPoints = maxPoints
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = _ui["Flash_AssignmentAdded"];
            return RedirectToAction(nameof(CourseAssignments), new { courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            var a = await _context.CourseAssignments.FindAsync(id);
            if (a != null && a.CourseId == courseId)
            {
                _context.CourseAssignments.Remove(a);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_AssignmentRemoved"];
            }
            return RedirectToAction(nameof(CourseAssignments), new { courseId });
        }
    }
}
