namespace UM_Project.Models
{
    public static class CalendarEventTypes
    {
        public const string Holiday = "Holiday";
        public const string ExamWeek = "ExamWeek";
        public const string NoSchool = "NoSchool";
        public const string Break = "Break";
    }

    public class SchoolCalendarEvent
    {
        public int SchoolCalendarEventId { get; set; }
        public DateTime EventDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = CalendarEventTypes.Holiday;
        public bool BlocksAttendance { get; set; } = true;
        public int? AcademicTermId { get; set; }
        public string? Notes { get; set; }
        public AcademicTerm? AcademicTerm { get; set; }
    }
}
