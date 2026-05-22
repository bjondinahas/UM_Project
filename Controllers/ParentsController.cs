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

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class ParentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountProvisioningService _provisioning;
        private readonly IAdminPasswordService _passwordService;
        private readonly AccountProvisioningSettings _settings;

        public ParentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAccountProvisioningService provisioning,
            IAdminPasswordService passwordService,
            IOptions<AccountProvisioningSettings> settings)
        {
            _context = context;
            _userManager = userManager;
            _provisioning = provisioning;
            _passwordService = passwordService;
            _settings = settings.Value;
        }

        public async Task<IActionResult> Index()
        {
            var rows = await _context.ParentGuardians
                .Include(p => p.Student)
                .OrderBy(p => p.FullName)
                .ToListAsync();

            var grouped = rows
                .GroupBy(p => string.IsNullOrEmpty(p.UserId) ? $"row:{p.ParentId}" : p.UserId)
                .Select(g =>
                {
                    var first = g.First();
                    return new ParentIndexViewModel
                    {
                        FullName = first.FullName,
                        Email = first.Email,
                        UserId = first.UserId,
                        RepresentativeParentId = first.ParentId,
                        Children = g.Select(x => new ParentChildLink
                        {
                            ParentId = x.ParentId,
                            StudentId = x.StudentId,
                            StudentName = x.Student?.FullName ?? "—",
                            StudentNumber = x.Student?.StudentNumber ?? "—"
                        }).ToList()
                    };
                })
                .OrderBy(p => p.FullName)
                .ToList();

            return View(grouped);
        }

        public async Task<IActionResult> Create(int? studentId)
        {
            await PopulateStudentListAsync(studentId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string parentFullName, string? parentEmail, int studentId)
        {
            if (string.IsNullOrWhiteSpace(parentFullName) || studentId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Parent name and student are required.");
                await PopulateStudentListAsync(studentId);
                return View();
            }

            var result = await _provisioning.LinkOrCreateParentAsync(
                parentFullName.Trim(), string.IsNullOrWhiteSpace(parentEmail) ? null : parentEmail.Trim(), studentId);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Could not create parent.");
                await PopulateStudentListAsync(studentId);
                return View();
            }

            var msg = string.IsNullOrEmpty(result.TemporaryPassword)
                ? $"Parent linked. Login: {result.Email}"
                : $"Parent created. Login: {result.Email} · Password: {result.TemporaryPassword}";
            TempData["Success"] = msg;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AssignChild(int? parentGuardianId, int? studentId)
        {
            await PopulateAssignChildListsAsync(parentGuardianId, studentId);
            if (studentId > 0)
            {
                var s = await _context.Students
                    .Include(x => x.Department)
                    .FirstOrDefaultAsync(x => x.StudentId == studentId);
                ViewBag.PreselectedStudentLabel = s != null
                    ? StudentSelectListHelper.FormatLabel(s)
                    : null;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignChild(int parentGuardianId, int studentId)
        {
            if (parentGuardianId <= 0 || studentId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Parent and student are required.");
                await PopulateAssignChildListsAsync(parentGuardianId, studentId);
                return View();
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
            {
                ModelState.AddModelError(string.Empty, "Student not found.");
                await PopulateAssignChildListsAsync(parentGuardianId, studentId);
                return View();
            }

            var result = await _provisioning.AssignChildToExistingParentAsync(parentGuardianId, studentId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Could not assign child.");
                await PopulateAssignChildListsAsync(parentGuardianId, studentId);
                return View();
            }

            TempData["Success"] = $"Linked {student.StudentNumber} ({student.FullName}) to parent.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var parent = await _context.ParentGuardians
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.ParentId == id);
            if (parent == null) return NotFound();

            var siblings = string.IsNullOrEmpty(parent.UserId)
                ? new List<ParentGuardian> { parent }
                : await _context.ParentGuardians
                    .Include(p => p.Student)
                    .Where(p => p.UserId == parent.UserId)
                    .ToListAsync();

            ViewBag.Siblings = siblings;
            ViewBag.StudentId = await StudentSelectListHelper.BuildAsync(_context, parent.StudentId);
            return View(parent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string fullName, string email, int studentId)
        {
            var parent = await _context.ParentGuardians.FindAsync(id);
            if (parent == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Name and email are required.");
                return await Edit(id);
            }

            email = email.Trim().ToLowerInvariant();
            fullName = fullName.Trim();

            if (!string.IsNullOrEmpty(parent.UserId))
            {
                var user = await _userManager.FindByIdAsync(parent.UserId);
                if (user != null && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    var taken = await _userManager.FindByEmailAsync(email);
                    if (taken != null && taken.Id != user.Id)
                    {
                        ModelState.AddModelError(string.Empty, "Email is already used by another account.");
                        return await Edit(id);
                    }

                    user.Email = email;
                    user.UserName = email;
                    user.FullName = fullName;
                    await _userManager.UpdateAsync(user);
                }
                else if (user != null)
                {
                    user.FullName = fullName;
                    await _userManager.UpdateAsync(user);
                }

                var sameUserRows = await _context.ParentGuardians
                    .Where(p => p.UserId == parent.UserId)
                    .ToListAsync();
                foreach (var row in sameUserRows)
                {
                    row.FullName = fullName;
                    row.Email = email;
                }
            }
            else
            {
                parent.FullName = fullName;
                parent.Email = email;
            }

            parent.StudentId = studentId;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Parent updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int parentId, string? newPassword, bool requireChange = true)
        {
            var parent = await _context.ParentGuardians.FindAsync(parentId);
            if (parent == null || string.IsNullOrEmpty(parent.UserId))
            {
                TempData["Error"] = "Parent has no login account.";
                return RedirectToAction(nameof(Index));
            }

            var pwd = string.IsNullOrWhiteSpace(newPassword) ? _settings.DefaultPassword : newPassword;
            var (ok, err) = await _passwordService.ResetPasswordAsync(parent.UserId, pwd, requireChange);
            TempData[ok ? "Success" : "Error"] = ok ? $"Password reset. New password: {pwd}" : err;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parent = await _context.ParentGuardians.FindAsync(id);
            if (parent == null)
                return RedirectToAction(nameof(Index));

            if (!string.IsNullOrEmpty(parent.UserId))
            {
                var otherLinks = await _context.ParentGuardians
                    .CountAsync(p => p.UserId == parent.UserId && p.ParentId != id);
                if (otherLinks == 0)
                {
                    var user = await _userManager.FindByIdAsync(parent.UserId);
                    if (user != null) await _userManager.DeleteAsync(user);
                }
            }

            _context.ParentGuardians.Remove(parent);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Parent link removed.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateStudentListAsync(int? studentId) =>
            ViewBag.StudentId = await StudentSelectListHelper.BuildAsync(_context, studentId);

        private async Task PopulateAssignChildListsAsync(int? parentGuardianId, int? studentId)
        {
            ViewBag.ParentGuardianId = await StudentSelectListHelper.BuildParentAccountSelectListAsync(
                _context, parentGuardianId);
            ViewBag.StudentId = await StudentSelectListHelper.BuildAsync(_context, studentId);
        }
    }
}
