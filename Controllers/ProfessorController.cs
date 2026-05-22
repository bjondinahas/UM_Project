using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using UM_Project.Data;

using UM_Project.Models;

using UM_Project.Services.Interfaces;
using UM_Project.Services;



namespace UM_Project.Controllers

{
    [Authorize(Roles = RoleNames.Professor)]

    public class ProfessorController : Controller

    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;

        private readonly IEmailService _emailService;

        private readonly ISystemSettingsService _settings;
        private readonly ISchoolCalendarService _calendar;

        public ProfessorController(
            ApplicationDbContext context,
            IEmailService emailService,
            ISystemSettingsService settings,
            ISchoolCalendarService calendar, IUiText ui)
        {
            _ui = ui;

            _context = context;
            _emailService = emailService;
            _settings = settings;
            _calendar = calendar;
        }



        private async Task<Professor?> GetCurrentProfessorAsync()

        {
            var userEmail = User.Identity?.Name;

            return await _context.Professors

                .Include(p => p.Department)

                .FirstOrDefaultAsync(p => p.Email == userEmail);

        }



        public async Task<IActionResult> Dashboard()

        {
            var professor = await GetCurrentProfessorAsync();

            if (professor == null) return View("NoProfile");



            var courses = await _context.Courses

                .Include(c => c.Department)

                .Where(c => c.ProfessorId == professor.ProfessorId)

                .ToListAsync();



            var courseIds = courses.Select(c => c.CourseId).ToList();

            var enrollmentCount = await _context.Enrollments.CountAsync(e => courseIds.Contains(e.CourseId));

            var grades = await _context.Grades.Where(g => courseIds.Contains(g.CourseId)).ToListAsync();

            var academic = await _settings.GetAcademicSettingsAsync();



            ViewBag.ProfessorName = professor.FullName;

            ViewBag.ProfessorEmail = professor.Email;

            ViewBag.ProfessorDepartment = professor.Department?.DepartmentName ?? "—";

            ViewBag.EnrollmentCount = enrollmentCount;

            ViewBag.AverageGrade = grades.Any() ? Math.Round(grades.Average(g => g.Value), 2) : 0;

            ViewBag.PassingStudents = grades.Where(g => g.Value >= academic.GradePassingMinimum).Select(g => g.StudentId).Distinct().Count();

            ViewBag.FailingStudents = grades.Where(g => g.Value < academic.GradePassingMinimum).Select(g => g.StudentId).Distinct().Count();

            ViewBag.GradePassingMin = academic.GradePassingMinimum;



            return View(courses);

        }



        public async Task<IActionResult> CourseStudents(int id)

        {
            var professor = await GetCurrentProfessorAsync();

            if (professor == null) return View("NoProfile");



            var course = await _context.Courses.Include(c => c.Department).FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            if (course.ProfessorId != professor.ProfessorId) return Forbid();



            var students = await _context.Enrollments

                .Include(e => e.Student)

                .Where(e => e.CourseId == id)

                .Select(e => e.Student!)

                .ToListAsync();

            var grades = await _context.Grades.Where(g => g.CourseId == id).ToDictionaryAsync(g => g.StudentId, g => g);

            var academic = await _settings.GetAcademicSettingsAsync();

            ViewBag.Course = course;

            ViewBag.Grades = grades;

            ViewBag.GradeMin = academic.GradeMinimum;

            ViewBag.GradeMax = academic.GradeMaximum;

            ViewBag.GradePassingMin = academic.GradePassingMinimum;

            return View(students);

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> SetGrade(int studentId, int courseId, int gradeValue)

        {
            var professor = await GetCurrentProfessorAsync();

            if (professor == null) return View("NoProfile");



            var course = await _context.Courses.FindAsync(courseId);

            if (course == null) return NotFound();

            if (course.ProfessorId != professor.ProfessorId) return Forbid();



            if (!await _settings.IsValidGradeAsync(gradeValue))

            {
                var academic = await _settings.GetAcademicSettingsAsync();

                TempData["Error"] = _ui.Format("Flash_GradeRange", academic.GradeMinimum, academic.GradeMaximum);

                return RedirectToAction(nameof(CourseStudents), new { id = courseId });

            }



            var existingGrade = await _context.Grades

                .FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseId == courseId);



            if (existingGrade != null)

            {
                TempData["Error"] = _ui["Flash_GradeAlreadyAssigned"];

                return RedirectToAction(nameof(CourseStudents), new { id = courseId });

            }



            _context.Add(new Grade

            {
                StudentId = studentId,

                CourseId = courseId,

                Value = gradeValue,

                DateRecorded = DateTime.UtcNow

            });

            await _context.SaveChangesAsync();



            var student = await _context.Students.FindAsync(studentId);

            if (student != null && !string.IsNullOrEmpty(student.Email))

                await _emailService.SendGradeNotificationAsync(student.Email, student.FullName, course.CourseName, gradeValue);



            TempData["Success"] = _ui["Flash_GradeSaved"];

            return RedirectToAction(nameof(CourseStudents), new { id = courseId });

        }



        public async Task<IActionResult> Attendance(int id, DateTime? date)

        {
            var professor = await GetCurrentProfessorAsync();

            if (professor == null) return View("NoProfile");



            var course = await _context.Courses.Include(c => c.Department).FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return NotFound();

            if (course.ProfessorId != professor.ProfessorId) return Forbid();



            var attendanceDate = (date ?? DateTime.Today).Date;
            var attendanceAllowed = await _calendar.IsAttendanceAllowedAsync(attendanceDate);
            ViewBag.AttendanceAllowed = attendanceAllowed;
            ViewBag.BlockReason = attendanceAllowed ? null : await _calendar.GetBlockReasonAsync(attendanceDate);

            var academic = await _settings.GetAcademicSettingsAsync();

            var slots = await _context.LessonSlots

                .Where(l => l.IsActive)

                .OrderBy(l => l.SlotNumber)

                .Take(academic.LessonsPerDay)

                .ToListAsync();



            var students = await _context.Enrollments

                .Include(e => e.Student)

                .Where(e => e.CourseId == id)

                .Select(e => e.Student!)

                .OrderBy(s => s.FullName)

                .ToListAsync();



            var existing = await _context.AttendanceRecords

                .Where(a => a.CourseId == id && a.AttendanceDate == attendanceDate)

                .ToListAsync();



            var absentKeys = new HashSet<string>();

            foreach (var r in existing.Where(x => !x.IsPresent))

                absentKeys.Add($"{r.StudentId}:{r.LessonSlotId}");



            var vm = new ProfessorAttendanceViewModel

            {
                Course = course,

                Date = attendanceDate,

                LessonSlots = slots,

                Students = students,

                AbsentKeys = absentKeys

            };

            return View(vm);

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> SaveAttendance(int courseId, DateTime date, string[]? absent)

        {
            var professor = await GetCurrentProfessorAsync();

            if (professor == null) return View("NoProfile");



            var course = await _context.Courses.FindAsync(courseId);

            if (course == null) return NotFound();

            if (course.ProfessorId != professor.ProfessorId) return Forbid();

            var attendanceDate = date.Date;
            if (!await _calendar.IsAttendanceAllowedAsync(attendanceDate))
            {
                TempData["Error"] = await _calendar.GetBlockReasonAsync(attendanceDate) ?? _ui["Flash_AttendanceBlocked"];
                return RedirectToAction(nameof(Attendance), new { id = courseId, date = attendanceDate.ToString("yyyy-MM-dd") });
            }

            var academic = await _settings.GetAcademicSettingsAsync();

            var slots = await _context.LessonSlots

                .Where(l => l.IsActive)

                .OrderBy(l => l.SlotNumber)

                .Take(academic.LessonsPerDay)

                .ToListAsync();



            var studentIds = await _context.Enrollments

                .Where(e => e.CourseId == courseId)

                .Select(e => e.StudentId)

                .ToListAsync();



            var absentSet = new HashSet<string>(absent ?? Array.Empty<string>(), StringComparer.Ordinal);



            foreach (var studentId in studentIds)

            {
                foreach (var slot in slots)

                {
                    var key = $"{studentId}:{slot.LessonSlotId}";

                    var isPresent = !absentSet.Contains(key);



                    var record = await _context.AttendanceRecords.FirstOrDefaultAsync(a =>

                        a.StudentId == studentId &&

                        a.CourseId == courseId &&

                        a.LessonSlotId == slot.LessonSlotId &&

                        a.AttendanceDate == attendanceDate);



                    if (record == null)

                    {
                        _context.AttendanceRecords.Add(new AttendanceRecord

                        {
                            StudentId = studentId,

                            CourseId = courseId,

                            LessonSlotId = slot.LessonSlotId,

                            AttendanceDate = attendanceDate,

                            IsPresent = isPresent,

                            ProfessorId = professor.ProfessorId,

                            MarkedAtUtc = DateTime.UtcNow

                        });

                    }

                    else

                    {
                        record.IsPresent = isPresent;

                        record.ProfessorId = professor.ProfessorId;

                        record.MarkedAtUtc = DateTime.UtcNow;

                    }

                }

            }



            await _context.SaveChangesAsync();

            var absentCount = absentSet.Count;

            TempData["Success"] = _ui.Format("Flash_AttendanceSaved", attendanceDate, absentCount);

            return RedirectToAction(nameof(Attendance), new { id = courseId, date = attendanceDate.ToString("yyyy-MM-dd") });

        }



        public async Task<IActionResult> Schedules()

        {
            var professor = await GetCurrentProfessorAsync();

            if (professor == null) return View("NoProfile");



            var schedules = await _context.Schedules

                .Include(s => s.Course)

                .Where(s => s.Course!.ProfessorId == professor.ProfessorId)

                .ToListAsync();



            var slots = await _context.LessonSlots.Where(l => l.IsActive).OrderBy(l => l.SlotNumber).ToListAsync();

            ViewBag.LessonSlots = slots;

            return View(schedules);

        }

    }

}


