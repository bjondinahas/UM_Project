using System.ComponentModel.DataAnnotations.Schema;

namespace UM_Project.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public int CourseId { get; set; }
        public string Day { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;

        public Course? Course { get; set; }
    }
}
