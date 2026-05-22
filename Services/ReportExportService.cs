using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UM_Project.Data;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services;

public class ReportExportService : IReportExportService
{
    private readonly ApplicationDbContext _db;
    private readonly IQrCodeService _qr;
    private readonly string _secret;

    public ReportExportService(ApplicationDbContext db, IQrCodeService qr, IConfiguration config)
    {
        _db = db;
        _qr = qr;
        _secret = config["Jwt:Secret"] ?? "UM_Report_Secret";
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public string BuildVerificationUrl(string reportType, string referenceId, string verifyBaseUrl)
    {
        var token = CreateToken(reportType, referenceId);
        return $"{verifyBaseUrl.TrimEnd('/')}/Reports/Verify?token={Uri.EscapeDataString(token)}";
    }

    public bool TryValidateToken(string token, out string? reportType, out string? referenceId)
    {
        reportType = null;
        referenceId = null;
        try
        {
            var bytes = Convert.FromBase64String(token.Replace('-', '+').Replace('_', '/'));
            var payload = Encoding.UTF8.GetString(bytes);
            var parts = payload.Split('|');
            if (parts.Length != 4) return false;
            reportType = parts[0];
            referenceId = parts[1];
            var sig = parts[3];
            var expected = Sign($"{parts[0]}|{parts[1]}|{parts[2]}");
            return sig == expected;
        }
        catch
        {
            return false;
        }
    }

    public async Task<byte[]> ClassAttendancePdfAsync(int courseId, DateTime from, DateTime to, string verifyBaseUrl)
    {
        var data = await LoadAttendanceAsync(courseId, from, to);
        var verifyUrl = BuildVerificationUrl("attendance", $"{courseId}:{from:yyyyMMdd}:{to:yyyyMMdd}", verifyBaseUrl);
        var qr = _qr.GeneratePng(verifyUrl, 6);
        return Document.Create(doc =>
        {
            doc.Page(p =>
            {
                p.Size(PageSizes.A4);
                p.Margin(40);
                p.Header().Text(t => { t.Span("Class Attendance Report").Bold().FontSize(18); });
                p.Content().Column(col =>
                {
                    col.Item().Text($"Course: {data.CourseName}");
                    col.Item().Text($"Period: {from:d} – {to:d}");
                    col.Item().PaddingTop(10).Text($"Present: {data.Present}  |  Absent: {data.Absent}  |  Late: {data.Late}");
                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("Student").Bold();
                            h.Cell().Text("Date").Bold();
                            h.Cell().Text("Slot").Bold();
                            h.Cell().Text("Status").Bold();
                        });
                        if (data.Rows.Count == 0)
                        {
                            table.Cell().ColumnSpan(4).Text("No attendance records for this period.").Italic();
                        }
                        else
                        {
                            foreach (var row in data.Rows.Take(200))
                            {
                                table.Cell().Text(row.StudentName);
                                table.Cell().Text(row.Date.ToString("d"));
                                table.Cell().Text(row.Slot);
                                table.Cell().Text(row.Status);
                            }
                        }
                    });
                    col.Item().PaddingTop(20).Row(r =>
                    {
                        r.ConstantItem(100).Image(qr);
                        r.RelativeItem().PaddingLeft(10).Text(t =>
                        {
                            t.Span("Scan to verify report authenticity.\n").FontSize(9);
                            t.Span(verifyUrl).FontSize(7);
                        });
                    });
                });
                p.Footer().AlignCenter().Text($"Generated {DateTime.UtcNow:u} UTC");
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> ClassAttendanceExcelAsync(int courseId, DateTime from, DateTime to)
    {
        var data = await LoadAttendanceAsync(courseId, from, to);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Attendance");
        ws.Cell(1, 1).Value = "Class Attendance Report";
        ws.Cell(2, 1).Value = "Course";
        ws.Cell(2, 2).Value = data.CourseName;
        ws.Cell(3, 1).Value = "Period";
        ws.Cell(3, 2).Value = $"{from:d} – {to:d}";
        var row = 5;
        ws.Cell(row, 1).Value = "Student";
        ws.Cell(row, 2).Value = "Date";
        ws.Cell(row, 3).Value = "Slot";
        ws.Cell(row, 4).Value = "Status";
        row++;
        foreach (var r in data.Rows)
        {
            ws.Cell(row, 1).Value = r.StudentName;
            ws.Cell(row, 2).Value = r.Date;
            ws.Cell(row, 3).Value = r.Slot;
            ws.Cell(row, 4).Value = r.Status;
            row++;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> DepartmentPerformancePdfAsync(int departmentId, int? termId, string verifyBaseUrl)
    {
        var data = await LoadDepartmentPerformanceAsync(departmentId, termId);
        var verifyUrl = BuildVerificationUrl("department", $"{departmentId}:{termId}", verifyBaseUrl);
        var qr = _qr.GeneratePng(verifyUrl, 6);
        return Document.Create(doc =>
        {
            doc.Page(p =>
            {
                p.Size(PageSizes.A4);
                p.Margin(40);
                p.Header().Text(t => t.Span("Department Performance").Bold().FontSize(18));
                p.Content().Column(col =>
                {
                    col.Item().Text($"Department: {data.DepartmentName}");
                    col.Item().Text($"Students: {data.StudentCount}  |  Avg grade: {data.AverageGrade:F2}");
                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); });
                        table.Header(h =>
                        {
                            h.Cell().Text("Course").Bold();
                            h.Cell().Text("Enrollments").Bold();
                            h.Cell().Text("Avg grade").Bold();
                        });
                        if (data.Courses.Count == 0)
                        {
                            table.Cell().ColumnSpan(3).Text("No courses in this department.").Italic();
                        }
                        else
                        {
                            foreach (var c in data.Courses)
                            {
                                table.Cell().Text(c.CourseName);
                                table.Cell().Text(c.EnrollmentCount.ToString());
                                table.Cell().Text(c.AverageGrade > 0 ? c.AverageGrade.ToString("F2", CultureInfo.InvariantCulture) : "—");
                            }
                        }
                    });
                    if (data.GradeBreakdown.Count > 0)
                    {
                        col.Item().PaddingTop(12).Text("Grade distribution (1–5)").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                            table.Header(h =>
                            {
                                h.Cell().Text("Grade").Bold();
                                h.Cell().Text("Count").Bold();
                            });
                            foreach (var g in data.GradeBreakdown)
                            {
                                table.Cell().Text(g.Grade.ToString());
                                table.Cell().Text(g.Count.ToString());
                            }
                        });
                    }
                    col.Item().PaddingTop(20).Row(r =>
                    {
                        r.ConstantItem(100).Image(qr);
                        r.RelativeItem().PaddingLeft(10).Text("Scan QR to verify.").FontSize(9);
                    });
                });
                p.Footer().AlignCenter().Text($"Generated {DateTime.UtcNow:u} UTC");
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> DepartmentPerformanceExcelAsync(int departmentId, int? termId)
    {
        var data = await LoadDepartmentPerformanceAsync(departmentId, termId);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Department");
        ws.Cell(1, 1).Value = "Department Performance";
        ws.Cell(2, 1).Value = data.DepartmentName;
        ws.Cell(3, 1).Value = "Avg grade";
        ws.Cell(3, 2).Value = data.AverageGrade;
        var row = 5;
        ws.Cell(row, 1).Value = "Course";
        ws.Cell(row, 2).Value = "Enrollments";
        ws.Cell(row, 3).Value = "Avg grade";
        row++;
        foreach (var c in data.Courses)
        {
            ws.Cell(row, 1).Value = c.CourseName;
            ws.Cell(row, 2).Value = c.EnrollmentCount;
            ws.Cell(row, 3).Value = c.AverageGrade;
            row++;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> TermSummaryPdfAsync(int termId, string verifyBaseUrl)
    {
        var data = await LoadTermSummaryAsync(termId);
        var verifyUrl = BuildVerificationUrl("term", termId.ToString(), verifyBaseUrl);
        var qr = _qr.GeneratePng(verifyUrl, 6);
        return Document.Create(doc =>
        {
            doc.Page(p =>
            {
                p.Size(PageSizes.A4);
                p.Margin(40);
                p.Header().Text(t => t.Span("Term Summary Report").Bold().FontSize(18));
                p.Content().Column(col =>
                {
                    col.Item().Text($"Term: {data.TermName} ({data.StartDate:d} – {data.EndDate:d})");
                    col.Item().Text($"Students enrolled: {data.StudentsEnrolled}");
                    col.Item().Text($"Grades recorded: {data.GradesCount}  |  Avg: {data.AverageGrade:F2} (scale 1–5)");
                    col.Item().Text($"Absences in term: {data.Absences}");
                    if (data.GradeBreakdown.Count > 0)
                    {
                        col.Item().PaddingTop(12).Text("Grade distribution").Bold();
                        foreach (var g in data.GradeBreakdown)
                            col.Item().Text($"Grade {g.Grade}: {g.Count} records");
                    }
                    if (data.CourseSummaries.Count > 0)
                    {
                        col.Item().PaddingTop(12).Text("Courses").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); });
                            table.Header(h =>
                            {
                                h.Cell().Text("Course").Bold();
                                h.Cell().Text("Grades").Bold();
                                h.Cell().Text("Avg").Bold();
                            });
                            foreach (var c in data.CourseSummaries)
                            {
                                table.Cell().Text(c.CourseName);
                                table.Cell().Text(c.EnrollmentCount.ToString());
                                table.Cell().Text(c.AverageGrade > 0 ? c.AverageGrade.ToString("F2", CultureInfo.InvariantCulture) : "—");
                            }
                        });
                    }
                    col.Item().PaddingTop(20).Row(r =>
                    {
                        r.ConstantItem(100).Image(qr);
                        r.RelativeItem().PaddingLeft(10).Text("Scan QR to verify.").FontSize(9);
                    });
                });
                p.Footer().AlignCenter().Text($"Generated {DateTime.UtcNow:u} UTC");
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> TermSummaryExcelAsync(int termId)
    {
        var data = await LoadTermSummaryAsync(termId);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Term");
        ws.Cell(1, 1).Value = "Term Summary";
        ws.Cell(2, 1).Value = data.TermName;
        ws.Cell(3, 1).Value = "Students";
        ws.Cell(3, 2).Value = data.StudentsEnrolled;
        ws.Cell(4, 1).Value = "Avg grade";
        ws.Cell(4, 2).Value = data.AverageGrade;
        ws.Cell(5, 1).Value = "Absences";
        ws.Cell(5, 2).Value = data.Absences;
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private string CreateToken(string reportType, string referenceId)
    {
        var ts = DateTime.UtcNow.Ticks.ToString();
        var sig = Sign($"{reportType}|{referenceId}|{ts}");
        var payload = $"{reportType}|{referenceId}|{ts}|{sig}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private string Sign(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    private async Task<AttendanceReportData> LoadAttendanceAsync(int courseId, DateTime from, DateTime to)
    {
        var course = await _db.Courses.FindAsync(courseId);
        var records = await _db.AttendanceRecords
            .Include(a => a.Student)
            .Include(a => a.LessonSlot)
            .Where(a => a.CourseId == courseId && a.AttendanceDate >= from.Date && a.AttendanceDate <= to.Date)
            .OrderBy(a => a.AttendanceDate)
            .ToListAsync();

        return new AttendanceReportData
        {
            CourseName = course?.CourseName ?? $"Course #{courseId}",
            Present = records.Count(r => r.IsPresent),
            Absent = records.Count(r => !r.IsPresent),
            Late = 0,
            Rows = records.Select(r => new AttendanceRow
            {
                StudentName = r.Student?.FullName ?? $"#{r.StudentId}",
                Date = r.AttendanceDate,
                Slot = r.LessonSlot?.Title ?? r.LessonSlotId.ToString(),
                Status = r.IsPresent ? "Present" : "Absent"
            }).ToList()
        };
    }

    private async Task<DepartmentReportData> LoadDepartmentPerformanceAsync(int departmentId, int? termId)
    {
        var dept = await _db.Departments.FindAsync(departmentId);
        var courses = await _db.Courses.Where(c => c.DepartmentId == departmentId).ToListAsync();
        var courseIds = courses.Select(c => c.CourseId).ToList();
        var grades = await _db.Grades.Where(g => courseIds.Contains(g.CourseId)).ToListAsync();
        if (termId.HasValue)
        {
            var term = await _db.AcademicTerms.FindAsync(termId.Value);
            if (term != null)
            {
                var termGrades = grades
                    .Where(g => g.DateRecorded >= term.StartDate && g.DateRecorded <= term.EndDate)
                    .ToList();
                if (termGrades.Count > 0)
                    grades = termGrades;
            }
        }

        var enrollmentCounts = await _db.Enrollments
            .Where(e => courseIds.Contains(e.CourseId))
            .GroupBy(e => e.CourseId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var courseStats = courses.Select(c =>
        {
            var cg = grades.Where(g => g.CourseId == c.CourseId).ToList();
            return new CourseStat
            {
                CourseName = c.CourseName,
                EnrollmentCount = enrollmentCounts.GetValueOrDefault(c.CourseId),
                AverageGrade = cg.Count > 0 ? cg.Average(g => g.Value) : 0
            };
        }).ToList();

        var gradeBreakdown = grades
            .GroupBy(g => g.Value)
            .OrderByDescending(g => g.Key)
            .Select(g => new GradeCountRow { Grade = g.Key, Count = g.Count() })
            .ToList();

        return new DepartmentReportData
        {
            DepartmentName = dept?.DepartmentName ?? $"Dept #{departmentId}",
            StudentCount = await _db.Students.CountAsync(s => s.DepartmentId == departmentId),
            AverageGrade = grades.Count > 0 ? grades.Average(g => g.Value) : 0,
            Courses = courseStats,
            GradeBreakdown = gradeBreakdown
        };
    }

    private async Task<TermSummaryData> LoadTermSummaryAsync(int termId)
    {
        var term = await _db.AcademicTerms.FindAsync(termId)
            ?? throw new InvalidOperationException("Term not found");
        var grades = await _db.Grades
            .Where(g => g.DateRecorded >= term.StartDate && g.DateRecorded <= term.EndDate)
            .ToListAsync();
        if (grades.Count == 0)
            grades = await _db.Grades.ToListAsync();

        var absences = await _db.AttendanceRecords
            .CountAsync(a => !a.IsPresent && a.AttendanceDate >= term.StartDate && a.AttendanceDate <= term.EndDate);

        var courseSummaries = await _db.Courses
            .Select(c => new
            {
                c.CourseId,
                c.CourseName,
                Grades = grades.Where(g => g.CourseId == c.CourseId).ToList()
            })
            .ToListAsync();

        return new TermSummaryData
        {
            TermName = term.Name,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            StudentsEnrolled = await _db.Enrollments.CountAsync(),
            GradesCount = grades.Count,
            AverageGrade = grades.Count > 0 ? grades.Average(g => g.Value) : 0,
            Absences = absences,
            GradeBreakdown = grades
                .GroupBy(g => g.Value)
                .OrderByDescending(g => g.Key)
                .Select(g => new GradeCountRow { Grade = g.Key, Count = g.Count() })
                .ToList(),
            CourseSummaries = courseSummaries
                .Where(c => c.Grades.Count > 0)
                .Select(c => new CourseStat
                {
                    CourseName = c.CourseName,
                    EnrollmentCount = c.Grades.Count,
                    AverageGrade = c.Grades.Average(g => g.Value)
                })
                .OrderByDescending(c => c.AverageGrade)
                .Take(15)
                .ToList()
        };
    }

    private sealed class AttendanceReportData
    {
        public string CourseName { get; set; } = "";
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public List<AttendanceRow> Rows { get; set; } = [];
    }

    private sealed class AttendanceRow
    {
        public string StudentName { get; set; } = "";
        public DateTime Date { get; set; }
        public string Slot { get; set; } = "";
        public string Status { get; set; } = "";
    }

    private sealed class DepartmentReportData
    {
        public string DepartmentName { get; set; } = "";
        public int StudentCount { get; set; }
        public double AverageGrade { get; set; }
        public List<CourseStat> Courses { get; set; } = [];
        public List<GradeCountRow> GradeBreakdown { get; set; } = [];
    }

    private sealed class GradeCountRow
    {
        public int Grade { get; set; }
        public int Count { get; set; }
    }

    private sealed class CourseStat
    {
        public string CourseName { get; set; } = "";
        public int EnrollmentCount { get; set; }
        public double AverageGrade { get; set; }
    }

    private sealed class TermSummaryData
    {
        public string TermName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StudentsEnrolled { get; set; }
        public int GradesCount { get; set; }
        public double AverageGrade { get; set; }
        public int Absences { get; set; }
        public List<GradeCountRow> GradeBreakdown { get; set; } = [];
        public List<CourseStat> CourseSummaries { get; set; } = [];
    }
}
