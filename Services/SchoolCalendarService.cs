using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class SchoolCalendarService : ISchoolCalendarService
    {
        private readonly ApplicationDbContext _context;

        public SchoolCalendarService(ApplicationDbContext context) => _context = context;

        public async Task<bool> IsAttendanceAllowedAsync(DateTime date)
        {
            var d = date.Date;
            if (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return false;

            var blocking = await _context.SchoolCalendarEvents
                .AnyAsync(e => e.EventDate == d && e.BlocksAttendance);
            return !blocking;
        }

        public async Task<string?> GetBlockReasonAsync(DateTime date)
        {
            var d = date.Date;
            if (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return "Weekend — no attendance";

            var ev = await _context.SchoolCalendarEvents
                .FirstOrDefaultAsync(e => e.EventDate == d && e.BlocksAttendance);
            return ev != null ? $"{ev.Title} ({ev.EventType})" : null;
        }

        public async Task<List<SchoolCalendarEvent>> GetEventsForMonthAsync(int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);
            return await _context.SchoolCalendarEvents
                .Include(e => e.AcademicTerm)
                .Where(e => e.EventDate >= start && e.EventDate < end)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<List<AcademicTerm>> GetTermsAsync() =>
            await _context.AcademicTerms.OrderByDescending(t => t.StartDate).ToListAsync();
    }
}
