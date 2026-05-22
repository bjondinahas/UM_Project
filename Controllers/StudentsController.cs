using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UM_Project.Data;
using UM_Project.Helpers;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class StudentsController : Controller
    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountProvisioningService _provisioning;
        private readonly IEmailService _emailService;
        private readonly IAdminPasswordService _passwordService;
        private readonly AccountProvisioningSettings _settings;
        private readonly ISystemSettingsService _academicSettings;

        public StudentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAccountProvisioningService provisioning,
            IEmailService emailService,
            IAdminPasswordService passwordService,
            IOptions<AccountProvisioningSettings> settings,
            ISystemSettingsService academicSettings,
            IUiText ui)
        {
            _ui = ui;

            _context = context;
            _academicSettings = academicSettings;
            _userManager = userManager;
            _provisioning = provisioning;
            _emailService = emailService;
            _passwordService = passwordService;
            _settings = settings.Value;
        }

        public async Task<IActionResult> Index() =>
            View(await _context.Students.Include(s => s.Department).OrderBy(s => s.FullName).ToListAsync());

        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.StudentId == id);
            if (student == null) return NotFound();

            var enrolledIds = await _context.Enrollments
                .Where(e => e.StudentId == id)
                .Select(e => e.CourseId)
                .ToListAsync();

            var grades = await _context.Grades
                .Include(g => g.Course).ThenInclude(c => c!.Professor)
                .Include(g => g.Course).ThenInclude(c => c!.Department)
                .Where(g => g.StudentId == id)
                .ToListAsync();

            var vm = new StudentDetailViewModel
            {
                Student = student,
                LoginUser = await _userManager.FindByIdAsync(student.UserId),
                Enrollments = await _context.Enrollments
                    .Include(e => e.Course).ThenInclude(c => c!.Professor)
                    .Include(e => e.Course).ThenInclude(c => c!.Department)
                    .Where(e => e.StudentId == id)
                    .ToListAsync(),
                Grades = grades,
                Parents = await _context.ParentGuardians
                    .Where(p => p.StudentId == id)
                    .ToListAsync(),
                Schedules = await _context.Schedules
                    .Include(s => s.Course)
                    .Where(s => enrolledIds.Contains(s.CourseId))
                    .ToListAsync(),
                AvailableCourses = await _context.Courses
                    .Include(c => c.Professor)
                    .Where(c => !enrolledIds.Contains(c.CourseId))
                    .OrderBy(c => c.CourseName)
                    .ToListAsync(),
                DefaultPassword = _settings.DefaultPassword
            };

            if (grades.Any())
            {
                var academic = await _academicSettings.GetAcademicSettingsAsync();
                vm.AverageGrade = Math.Round(grades.Average(g => g.Value), 2);
                vm.PassedCount = grades.Count(g => g.Value >= academic.GradePassingMinimum);
                vm.FailedCount = grades.Count(g => g.Value < academic.GradePassingMinimum);
                ViewBag.GradePassingMin = academic.GradePassingMinimum;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollCourse(int studentId, int courseId)
        {
            if (await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId))
            {
                TempData["Error"] = _ui["Flash_AlreadyEnrolled"];
                return RedirectToAction(nameof(Details), new { id = studentId });
            }

            _context.Enrollments.Add(new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = _ui["Flash_Enrolled"];
            return RedirectToAction(nameof(Details), new { id = studentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnenrollCourse(int studentId, int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.StudentId == studentId);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_EnrollmentRemoved"];
            }
            return RedirectToAction(nameof(Details), new { id = studentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkParent(int studentId, string parentFullName, string? parentEmail)
        {
            if (string.IsNullOrWhiteSpace(parentFullName))
            {
                TempData["Error"] = _ui["Flash_ParentNameRequired"];
                return RedirectToAction(nameof(Details), new { id = studentId });
            }

            var result = await _provisioning.LinkOrCreateParentAsync(
                parentFullName.Trim(), string.IsNullOrWhiteSpace(parentEmail) ? null : parentEmail.Trim(), studentId);
            if (!result.Success)
            {
                TempData["Error"] = result.Error ?? _ui["Flash_CouldNotCreateParent"];
                return RedirectToAction(nameof(Details), new { id = studentId });
            }

            TempData["Success"] = string.IsNullOrEmpty(result.TemporaryPassword)
                ? _ui.Format("Flash_ParentLinked", result.Email)
                : _ui.Format("Flash_ParentLinkedPwd", result.Email, result.TemporaryPassword);
            return RedirectToAction(nameof(Details), new { id = studentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int studentId, string? newPassword, bool requireChange = true)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return NotFound();

            var pwd = string.IsNullOrWhiteSpace(newPassword) ? _settings.DefaultPassword : newPassword;
            var (ok, err) = await _passwordService.ResetPasswordAsync(student.UserId, pwd, requireChange);
            TempData[ok ? "Success" : "Error"] = ok
                ? (requireChange ? _ui.Format("Flash_PasswordResetForce", pwd) : _ui.Format("Flash_PasswordReset", pwd))
                : err;
            return RedirectToAction(nameof(Details), new { id = studentId });
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.ExistingParents = await StudentSelectListHelper.BuildParentAccountSelectListAsync(_context);
            return View(new StudentRegistrationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentRegistrationViewModel model)
        {
            if (model.AddParent && string.IsNullOrWhiteSpace(model.ParentFullName))
                ModelState.AddModelError(nameof(model.ParentFullName), _ui["Flash_ParentWhenAdding"]);

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
                ViewBag.ExistingParents = await StudentSelectListHelper.BuildParentAccountSelectListAsync(_context);
                return View(model);
            }

            var result = await _provisioning.ProvisionStudentAsync(model.FullName.Trim(), model.DepartmentId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? _ui["Flash_CouldNotCreateStudent"]);
                ViewBag.Departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
                ViewBag.ExistingParents = await StudentSelectListHelper.BuildParentAccountSelectListAsync(_context);
                return View(model);
            }

            await _emailService.SendWelcomeEmailAsync(
                result.Email, model.FullName.Trim(), result.TemporaryPassword, RoleNames.Student);

            var created = await _context.Students
                .Include(s => s.Department)
                .FirstAsync(s => s.UserId == result.UserId);

            var messages = new List<string>
            {
                _ui.Format("Flash_StudentCreated", created.StudentNumber, result.Email, result.TemporaryPassword)
            };

            if (model.AddParent)
            {
                var parentResult = await _provisioning.LinkOrCreateParentAsync(
                    model.ParentFullName!.Trim(),
                    string.IsNullOrWhiteSpace(model.ParentEmail) ? null : model.ParentEmail.Trim(),
                    created.StudentId);

                if (parentResult.Success)
                {
                    messages.Add(string.IsNullOrEmpty(parentResult.TemporaryPassword)
                        ? _ui.Format("Flash_ParentLinked", parentResult.Email)
                        : _ui.Format("Flash_ParentCreatedLinked", parentResult.Email, parentResult.TemporaryPassword));
                }
                else
                    messages.Add(_ui.Format("Flash_ParentNotLinked", parentResult.Error));
            }

            TempData["Success"] = string.Join(" · ", messages);
            TempData["ShowCredentials"] = true;
            return RedirectToAction(nameof(Details), new { id = created.StudentId });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.StudentId) return NotFound();
            ModelState.Remove("Department");
            ModelState.Remove("Enrollments");
            ModelState.Remove("Grades");
            ModelState.Remove(nameof(student.Email));
            ModelState.Remove(nameof(student.StudentNumber));
            ModelState.Remove(nameof(student.UserId));

            if (ModelState.IsValid)
            {
                var existing = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == id);
                if (existing == null) return NotFound();

                student.Email = existing.Email;
                student.StudentNumber = existing.StudentNumber;
                student.UserId = existing.UserId;
                _context.Update(student);
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(existing.UserId);
                if (user != null)
                {
                    user.FullName = student.FullName;
                    user.CustomId = student.StudentNumber;
                    await _userManager.UpdateAsync(user);
                }

                TempData["Success"] = _ui["Flash_StudentUpdated"];
                return RedirectToAction(nameof(Details), new { id });
            }
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                var parents = await _context.ParentGuardians.Where(p => p.StudentId == id).ToListAsync();
                foreach (var p in parents)
                {
                    if (!string.IsNullOrEmpty(p.UserId))
                    {
                        var otherChildren = await _context.ParentGuardians
                            .CountAsync(g => g.UserId == p.UserId && g.StudentId != id);
                        if (otherChildren == 0)
                        {
                            var pu = await _userManager.FindByIdAsync(p.UserId);
                            if (pu != null) await _userManager.DeleteAsync(pu);
                        }
                    }
                    _context.ParentGuardians.Remove(p);
                }

                if (!string.IsNullOrEmpty(student.UserId))
                {
                    var user = await _userManager.FindByIdAsync(student.UserId);
                    if (user != null) await _userManager.DeleteAsync(user);
                }
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_StudentDeleted"];
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
