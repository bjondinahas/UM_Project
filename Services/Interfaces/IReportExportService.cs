namespace UM_Project.Services.Interfaces;

public interface IReportExportService
{
    Task<byte[]> ClassAttendancePdfAsync(int courseId, DateTime from, DateTime to, string verifyBaseUrl);
    Task<byte[]> ClassAttendanceExcelAsync(int courseId, DateTime from, DateTime to);
    Task<byte[]> DepartmentPerformancePdfAsync(int departmentId, int? termId, string verifyBaseUrl);
    Task<byte[]> DepartmentPerformanceExcelAsync(int departmentId, int? termId);
    Task<byte[]> TermSummaryPdfAsync(int termId, string verifyBaseUrl);
    Task<byte[]> TermSummaryExcelAsync(int termId);
    string BuildVerificationUrl(string reportType, string referenceId, string verifyBaseUrl);
    bool TryValidateToken(string token, out string? reportType, out string? referenceId);
}
