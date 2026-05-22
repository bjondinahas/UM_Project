namespace UM_Project.Models
{
    public class ProfessorAttendanceViewModel
    {
        public Course Course { get; set; } = null!;
        public DateTime Date { get; set; }
        public List<LessonSlot> LessonSlots { get; set; } = new();
        public List<Student> Students { get; set; } = new();
        /// <summary>Key: $"{StudentId}:{LessonSlotId}" → absent (not present)</summary>
        public HashSet<string> AbsentKeys { get; set; } = new();
    }

    public class ParentAttendanceCalendarViewModel
    {
        public Student Student { get; set; } = null!;
        public int Year { get; set; }
        public int Month { get; set; }
        public List<LessonSlot> LessonSlots { get; set; } = new();
        public List<ParentDayAttendance> Days { get; set; } = new();
        public List<ParentGuardian> LinkedChildren { get; set; } = new();
        public int SelectedStudentId { get; set; }
    }

    public class ParentDayAttendance
    {
        public DateTime Date { get; set; }
        public int TotalSlots { get; set; }
        public int AbsentCount { get; set; }
        public List<ParentAbsenceDetail> Details { get; set; } = new();
    }

    public class ParentAbsenceDetail
    {
        public string CourseName { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
    }
}
