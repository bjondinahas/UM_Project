using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Controllers;

[Authorize(Roles = RoleNames.AdminPanel)]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IReportExportService _reports;
    private readonly IAdminAuditService _audit;

    public ReportsController(ApplicationDbContext db, IReportExportService reports, IAdminAuditService audit)
    {
        _db = db;
        _reports = reports;
        _audit = audit;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Courses = await _db.Courses.OrderBy(c => c.CourseName).ToListAsync();
        ViewBag.Departments = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
        ViewBag.Terms = await _db.AcademicTerms.OrderByDescending(t => t.StartDate).ToListAsync();
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Verify(string? token)
    {
        if (string.IsNullOrEmpty(token) || !_reports.TryValidateToken(token, out var type, out var reference))
            return Content("Invalid or expired verification token.", "text/plain");

        return Content($"Valid University Manager report.\nType: {type}\nReference: {reference}", "text/plain");
    }

    private string BaseUrl => $"{Request.Scheme}://{Request.Host}";

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AttendancePdf(int courseId, DateTime from, DateTime to)
    {
        var bytes = await _reports.ClassAttendancePdfAsync(courseId, from, to, BaseUrl);
        await _audit.LogAsync(HttpContext, AdminActions.ReportExport, AuditEntityTypes.Report,
            $"attendance-{courseId}", $"PDF {from:d}-{to:d}");
        return File(bytes, "application/pdf", $"Attendance_{courseId}.pdf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AttendanceExcel(int courseId, DateTime from, DateTime to)
    {
        var bytes = await _reports.ClassAttendanceExcelAsync(courseId, from, to);
        await _audit.LogAsync(HttpContext, AdminActions.ReportExport, AuditEntityTypes.Report,
            $"attendance-{courseId}", $"Excel {from:d}-{to:d}");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Attendance_{courseId}.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DepartmentPdf(int departmentId, int? termId)
    {
        var bytes = await _reports.DepartmentPerformancePdfAsync(departmentId, termId, BaseUrl);
        await _audit.LogAsync(HttpContext, AdminActions.ReportExport, AuditEntityTypes.Report,
            $"dept-{departmentId}", "PDF department performance");
        return File(bytes, "application/pdf", $"Department_{departmentId}.pdf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DepartmentExcel(int departmentId, int? termId)
    {
        var bytes = await _reports.DepartmentPerformanceExcelAsync(departmentId, termId);
        await _audit.LogAsync(HttpContext, AdminActions.ReportExport, AuditEntityTypes.Report,
            $"dept-{departmentId}", "Excel department performance");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Department_{departmentId}.xlsx");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TermPdf(int termId)
    {
        var bytes = await _reports.TermSummaryPdfAsync(termId, BaseUrl);
        await _audit.LogAsync(HttpContext, AdminActions.ReportExport, AuditEntityTypes.Report,
            $"term-{termId}", "PDF term summary");
        return File(bytes, "application/pdf", $"Term_{termId}.pdf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TermExcel(int termId)
    {
        var bytes = await _reports.TermSummaryExcelAsync(termId);
        await _audit.LogAsync(HttpContext, AdminActions.ReportExport, AuditEntityTypes.Report,
            $"term-{termId}", "Excel term summary");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Term_{termId}.xlsx");
    }
}
