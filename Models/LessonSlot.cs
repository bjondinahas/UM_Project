namespace UM_Project.Models
{
    /// <summary>Daily lesson period (e.g. 6 slots per school day) configured by SuperAdmin.</summary>
    public class LessonSlot
    {
        public int LessonSlotId { get; set; }
        public int SlotNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
