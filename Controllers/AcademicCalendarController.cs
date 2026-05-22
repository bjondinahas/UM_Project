using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize]
    public class AcademicCalendarController : Controller
    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;
        private readonly ISchoolCalendarService _calendar;
        private readonly IAdminAuditService _audit;

        public AcademicCalendarController(ApplicationDbContext context, ISchoolCalendarService calendar, IAdminAuditService audit, IUiText ui)
        {
            _ui = ui;

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

        [Authorize(Roles = RoleNames.AdminPanel)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTerm(string name, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = _ui["Flash_TermNameRequired"];
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
            TempData["Success"] = _ui["Flash_TermAdded"];
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = RoleNames.AdminPanel)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEvent(DateTime eventDate, string title, string eventType, bool blocksAttendance, int? academicTermId, string? notes)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Error"] = _ui["Flash_EventTitleRequired"];
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
            TempData["Success"] = _ui["Flash_EventAdded"];
            return RedirectToAction(nameof(Index), new { year = eventDate.Year, month = eventDate.Month });
        }

        [Authorize(Roles = RoleNames.AdminPanel)]
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
                TempData["Success"] = _ui["Flash_EventRemoved"];
            }
            return RedirectToAction(nameof(Index), new { year, month });
        }
    }
}
