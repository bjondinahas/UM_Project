using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class AcademicCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISchoolCalendarService _calendar;
        private readonly IAdminAuditService _audit;

        public AcademicCalendarController(ApplicationDbContext context, ISchoolCalendarService calendar, IAdminAuditService audit)
        {
            _context = context;
            _calendar = calendar;
            _audit = audit;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var y = year ?? DateTime.Today.Year;
            var m = month ?? DateTime.Today.Month;
            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.MonthName = new DateTime(y, m, 1).ToString("MMMM yyyy");
            ViewBag.Events = await _calendar.GetEventsForMonthAsync(y, m);
            ViewBag.Terms = await _calendar.GetTermsAsync();
            ViewBag.DaysInMonth = DateTime.DaysInMonth(y, m);
            ViewBag.FirstDayOfWeek = (int)new DateTime(y, m, 1).DayOfWeek;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTerm(string name, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Term name is required.";
                return RedirectToAction(nameof(Index));
            }
            _context.AcademicTerms.Add(new AcademicTerm
            {
                Name = name.Trim(),
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                IsActive = true
            });
            await _context.SaveChangesAsync();
            await _audit.LogAsync(HttpContext, AdminActions.CalendarChange, AuditEntityTypes.Calendar,
                null, $"AddTerm: {name.Trim()}");
            TempData["Success"] = "Term added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEvent(DateTime eventDate, string title, string eventType, bool blocksAttendance, int? academicTermId, string? notes)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = "Event title is required.";
                return RedirectToAction(nameof(Index), new { year = eventDate.Year, month = eventDate.Month });
            }
            _context.SchoolCalendarEvents.Add(new SchoolCalendarEvent
            {
                EventDate = eventDate.Date,
                Title = title.Trim(),
                EventType = string.IsNullOrWhiteSpace(eventType) ? CalendarEventTypes.Holiday : eventType,
                BlocksAttendance = blocksAttendance,
                AcademicTermId = academicTermId > 0 ? academicTermId : null,
                Notes = notes
            });
            await _context.SaveChangesAsync();
            await _audit.LogAsync(HttpContext, AdminActions.CalendarChange, AuditEntityTypes.Calendar,
                eventDate.ToString("yyyy-MM-dd"), $"AddEvent: {title.Trim()}");
            TempData["Success"] = "Calendar event added.";
            return RedirectToAction(nameof(Index), new { year = eventDate.Year, month = eventDate.Month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id, int year, int month)
        {
            var ev = await _context.SchoolCalendarEvents.FindAsync(id);
            if (ev != null)
            {
                _context.SchoolCalendarEvents.Remove(ev);
                await _context.SaveChangesAsync();
                await _audit.LogAsync(HttpContext, AdminActions.CalendarChange, AuditEntityTypes.Calendar,
                    id.ToString(), $"DeleteEvent: {ev.Title}");
                TempData["Success"] = "Event removed.";
            }
            return RedirectToAction(nameof(Index), new { year, month });
        }
    }
}
