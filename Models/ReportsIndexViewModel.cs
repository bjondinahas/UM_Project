namespace UM_Project.Models;

public class ReportsIndexViewModel
{
    public List<Course> Courses { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<AcademicTerm> Terms { get; set; } = new();

    public int GradeMinimum { get; set; } = 1;
    public int GradeMaximum { get; set; } = 5;
    public int GradePassingMinimum { get; set; } = 3;

    public List<string> GradeChartLabels { get; set; } = new();
    public List<int> GradeChartCounts { get; set; } = new();

    public string? PreviewCourseName { get; set; }
    public int PreviewAttendancePresent { get; set; }
    public int PreviewAttendanceAbsent { get; set; }
    public int PreviewAttendanceRows { get; set; }

    public string? PreviewDepartmentName { get; set; }
    public int PreviewDepartmentStudents { get; set; }
    public double PreviewDepartmentAvgGrade { get; set; }
    public List<ReportCoursePreviewRow> PreviewDepartmentCourses { get; set; } = new();

    public string? PreviewTermName { get; set; }
    public int PreviewTermGradesCount { get; set; }
    public double PreviewTermAvgGrade { get; set; }
    public int PreviewTermAbsences { get; set; }
}

public class ReportCoursePreviewRow
{
    public string CourseName { get; set; } = "";
    public int Enrollments { get; set; }
    public double AverageGrade { get; set; }
}
