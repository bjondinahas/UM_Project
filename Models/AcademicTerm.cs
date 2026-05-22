namespace UM_Project.Models
{
    public class AcademicTerm
    {
        public int AcademicTermId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<SchoolCalendarEvent> Events { get; set; } = new List<SchoolCalendarEvent>();
    }
}
