using UM_Project.Models;

namespace UM_Project.Services.Interfaces
{
    public interface ISchoolCalendarService
    {
        Task<bool> IsAttendanceAllowedAsync(DateTime date);
        Task<string?> GetBlockReasonAsync(DateTime date);
        Task<List<SchoolCalendarEvent>> GetEventsForMonthAsync(int year, int month);
        Task<List<AcademicTerm>> GetTermsAsync();
    }
}
