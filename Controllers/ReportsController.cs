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

    private readonly ISystemSettingsService _settings;



    public ReportsController(

        ApplicationDbContext db,

        IReportExportService reports,

        IAdminAuditService audit,

        ISystemSettingsService settings)

    {

        _db = db;

        _reports = reports;

        _audit = audit;

        _settings = settings;

    }



    public async Task<IActionResult> Index()

    {

        var academic = await _settings.GetAcademicSettingsAsync();

        var courses = await _db.Courses.OrderBy(c => c.CourseName).ToListAsync();

        var departments = await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();

        var terms = await _db.AcademicTerms.OrderByDescending(t => t.StartDate).ToListAsync();



        var vm = new ReportsIndexViewModel

        {

            Courses = courses,

            Departments = departments,

            Terms = terms,

            GradeMinimum = academic.GradeMinimum,

            GradeMaximum = academic.GradeMaximum,

            GradePassingMinimum = academic.GradePassingMinimum

        };



        for (var v = academic.GradeMaximum; v >= academic.GradeMinimum; v--)

        {

            vm.GradeChartLabels.Add(v.ToString());

            vm.GradeChartCounts.Add(await _db.Grades.CountAsync(g => g.Value == v));

        }



        var previewCourse = courses.FirstOrDefault();

        if (previewCourse != null)

        {

            var to = DateTime.Today;

            var from = to.AddDays(-60);

            var records = await _db.AttendanceRecords

                .Where(a => a.CourseId == previewCourse.CourseId

                    && a.AttendanceDate >= from.Date

                    && a.AttendanceDate <= to.Date)

                .ToListAsync();

            vm.PreviewCourseName = previewCourse.CourseName;

            vm.PreviewAttendancePresent = records.Count(r => r.IsPresent);

            vm.PreviewAttendanceAbsent = records.Count(r => !r.IsPresent);

            vm.PreviewAttendanceRows = records.Count;

        }



        var previewDept = departments.FirstOrDefault();

        if (previewDept != null)

        {

            var deptCourses = await _db.Courses.Where(c => c.DepartmentId == previewDept.DepartmentId).ToListAsync();

            var courseIds = deptCourses.Select(c => c.CourseId).ToList();

            var grades = await _db.Grades.Where(g => courseIds.Contains(g.CourseId)).ToListAsync();

            var enrollmentCounts = await _db.Enrollments

                .Where(e => courseIds.Contains(e.CourseId))

                .GroupBy(e => e.CourseId)

                .Select(g => new { g.Key, Count = g.Count() })

                .ToDictionaryAsync(x => x.Key, x => x.Count);



            vm.PreviewDepartmentName = previewDept.DepartmentName;

            vm.PreviewDepartmentStudents = await _db.Students.CountAsync(s => s.DepartmentId == previewDept.DepartmentId);

            vm.PreviewDepartmentAvgGrade = grades.Count > 0 ? grades.Average(g => g.Value) : 0;

            vm.PreviewDepartmentCourses = deptCourses.Select(c =>

            {

                var cg = grades.Where(g => g.CourseId == c.CourseId).ToList();

                return new ReportCoursePreviewRow

                {

                    CourseName = c.CourseName,

                    Enrollments = enrollmentCounts.GetValueOrDefault(c.CourseId),

                    AverageGrade = cg.Count > 0 ? cg.Average(g => g.Value) : 0

                };

            }).ToList();

        }



        var previewTerm = terms.FirstOrDefault();

        if (previewTerm != null)

        {

            var termGrades = await _db.Grades

                .Where(g => g.DateRecorded >= previewTerm.StartDate && g.DateRecorded <= previewTerm.EndDate)

                .ToListAsync();

            if (termGrades.Count == 0)

                termGrades = await _db.Grades.ToListAsync();



            vm.PreviewTermName = previewTerm.Name;

            vm.PreviewTermGradesCount = termGrades.Count;

            vm.PreviewTermAvgGrade = termGrades.Count > 0 ? termGrades.Average(g => g.Value) : 0;

            vm.PreviewTermAbsences = await _db.AttendanceRecords.CountAsync(a =>

                !a.IsPresent

                && a.AttendanceDate >= previewTerm.StartDate

                && a.AttendanceDate <= previewTerm.EndDate);

        }



        return View(vm);

    }



    [HttpGet]

    [AllowAnonymous]

    public IActionResult Verify(string? token)

    {

        if (string.IsNullOrEmpty(token) || !_reports.TryValidateToken(token, out var type, out var reference))

            return Content("Shenja e verifikimit e pavlefshme ose e skaduar.", "text/plain");

        return Content($"Raport i vlefshëm — Menaxhimi Shkollor.\nLloji: {type}\nReferenca: {reference}", "text/plain");

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

