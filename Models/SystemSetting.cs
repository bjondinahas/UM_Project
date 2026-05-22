namespace UM_Project.Models
{
    public class SystemSetting
    {
        public int SystemSettingId { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public static class SystemSettingKeys
    {
        public const string GradeMinimum = "GradeMinimum";
        public const string GradeMaximum = "GradeMaximum";
        public const string GradePassingMinimum = "GradePassingMinimum";
        public const string LessonsPerDay = "LessonsPerDay";
        public const string AtRiskAbsenceDays = "AtRiskAbsenceDays";
        public const string AtRiskAbsenceCount = "AtRiskAbsenceCount";
    }

    public class AcademicSettingsDto
    {
        public int GradeMinimum { get; set; } = 5;
        public int GradeMaximum { get; set; } = 10;
        public int GradePassingMinimum { get; set; } = 6;
        public int LessonsPerDay { get; set; } = 6;
    }
}
