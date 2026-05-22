using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class SchedulesController : Controller
    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;
        public SchedulesController(ApplicationDbContext context, IUiText ui)
        {
            _ui = ui;

            _context = context;
        }

        private async Task<string> CheckScheduleConflicts(Schedule schedule, int? excludeId = null)
        {
            var sameCourseSameTime = await _context.Schedules
                .AnyAsync(s => s.CourseId == schedule.CourseId && s.Day == schedule.Day && s.Time == schedule.Time && s.ScheduleId != excludeId);
            if (sameCourseSameTime) return _ui["Flash_ScheduleDuplicate"];

            var course = await _context.Courses.FindAsync(schedule.CourseId);
            if (course == null) return _ui["Flash_CourseNotFound"];

            var professorCourses = await _context.Courses
                .Where(c => c.ProfessorId == course.ProfessorId && c.CourseId != schedule.CourseId)
                .Select(c => c.CourseId)
                .ToListAsync();

            var professorSchedules = await _context.Schedules
                .Where(s => professorCourses.Contains(s.CourseId) && s.Day == schedule.Day && s.Time == schedule.Time && s.ScheduleId != excludeId)
                .ToListAsync();
            if (professorSchedules.Any()) return _ui["Flash_ProfessorBusy"];

            return null;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _context.Schedules.Include(s => s.Course).ToListAsync();
            return View(schedules);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            ModelState.Remove("Course");
            var conflict = await CheckScheduleConflicts(schedule);
            if (conflict != null)
            {
                TempData["Error"] = conflict;
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(schedule);
            }

            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_ScheduleCreated"];
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View(schedule);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null) return NotFound();
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            if (id != schedule.ScheduleId) return NotFound();
            ModelState.Remove("Course");
            var conflict = await CheckScheduleConflicts(schedule, id);
            if (conflict != null)
            {
                TempData["Error"] = conflict;
                ViewBag.Courses = await _context.Courses.ToListAsync();
                return View(schedule);
            }
            if (ModelState.IsValid)
            {
                _context.Update(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_ScheduleUpdated"];
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View(schedule);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.Schedules.Include(s => s.Course).FirstOrDefaultAsync(s => s.ScheduleId == id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

      [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_ScheduleDeleted"];
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
