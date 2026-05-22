namespace UM_Project.Models
{
    public class AnalyticsFilterInput
    {
        public int? DepartmentId { get; set; }
        public int? CourseId { get; set; }
        public int? ProfessorId { get; set; }
        public int? GradeValue { get; set; }
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; }
    }

    public class AnalyticsViewModel
    {
        public AnalyticsFilterInput Filter { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<Course> Courses { get; set; } = new();
        public List<Professor> Professors { get; set; } = new();

        public int TotalStudents { get; set; }
        public int TotalProfessors { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalGrades { get; set; }
        public double AverageGrade { get; set; }
        public int PassingStudents { get; set; }
        public int FailingStudents { get; set; }

        public int GradeMinimum { get; set; } = 1;
        public int GradeMaximum { get; set; } = 5;
        public int GradePassingMinimum { get; set; } = 3;
        public List<int> GradeHistogram { get; set; } = new();
        public List<string> GradeHistogramLabels { get; set; } = new();

        public List<string> DeptLabels { get; set; } = new();
        public List<int> DeptCounts { get; set; } = new();
        public List<string> CourseLabels { get; set; } = new();
        public List<int> CourseCounts { get; set; } = new();

        public List<AnalyticsGradeRow> GradeRows { get; set; } = new();
        public List<AnalyticsStudentRow> StudentRows { get; set; } = new();
    }

    public class AnalyticsGradeRow
    {
        public string StudentName { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string ProfessorName { get; set; } = "";
        public int Value { get; set; }
        public string Status { get; set; } = "";
        public DateTime DateRecorded { get; set; }
    }

    public class AnalyticsStudentRow
    {
        public string FullName { get; set; } = "";
        public string StudentNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public int EnrollmentCount { get; set; }
        public double? AverageGrade { get; set; }
    }
}
