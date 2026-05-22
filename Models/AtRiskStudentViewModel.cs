namespace UM_Project.Models
{
    public class AtRiskStudentViewModel
    {
        public Student Student { get; set; } = null!;
        public double AverageGrade { get; set; }
        public int AbsencesLast30Days { get; set; }
        public int LowGradeCount { get; set; }
        public List<string> Reasons { get; set; } = new();
        public List<InterventionNote> Notes { get; set; } = new();
    }
}
