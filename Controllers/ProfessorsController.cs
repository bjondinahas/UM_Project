using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class ProfessorsController : Controller
    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountProvisioningService _provisioning;
        private readonly IEmailService _emailService;
        private readonly IAdminPasswordService _passwordService;
        private readonly AccountProvisioningSettings _settings;

        public ProfessorsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAccountProvisioningService provisioning,
            IEmailService emailService,
            IAdminPasswordService passwordService,
            IOptions<AccountProvisioningSettings> settings, IUiText ui)
        {
            _ui = ui;

            _context = context;
            _userManager = userManager;
            _provisioning = provisioning;
            _emailService = emailService;
            _passwordService = passwordService;
            _settings = settings.Value;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int professorId, string? newPassword, bool requireChange = true)
        {
            var prof = await _context.Professors.FindAsync(professorId);
            if (prof == null || string.IsNullOrEmpty(prof.UserId))
            {
                TempData["Error"] = _ui["Flash_ProfessorNoLogin"];
                return RedirectToAction(nameof(Index));
            }

            var pwd = string.IsNullOrWhiteSpace(newPassword) ? _settings.DefaultPassword : newPassword;
            var (ok, err) = await _passwordService.ResetPasswordAsync(prof.UserId, pwd, requireChange);
            TempData[ok ? "Success" : "Error"] = ok ? _ui.Format("Flash_PasswordReset", pwd) : err;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index() => View(await _context.Professors.Include(p => p.Department).ToListAsync());

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.ProvisioningInfo = true;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Professor professor)
        {
            ModelState.Remove(nameof(professor.Email));
            ModelState.Remove(nameof(professor.UserId));
            ModelState.Remove("Department");
            ModelState.Remove("Courses");

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Departments.ToListAsync();
                ViewBag.ProvisioningInfo = true;
                return View(professor);
            }

            var result = await _provisioning.ProvisionProfessorAsync(
                professor.FullName, professor.DepartmentId, professor.Title);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Could not create professor account.");
                ViewBag.Departments = await _context.Departments.ToListAsync();
                ViewBag.ProvisioningInfo = true;
                return View(professor);
            }

            await _emailService.SendWelcomeEmailAsync(
                result.Email, professor.FullName, result.TemporaryPassword, RoleNames.Professor);

            TempData["Success"] = _ui.Format("Flash_ProfessorCreated", result.Email, result.GeneratedId, result.TemporaryPassword);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var prof = await _context.Professors.FindAsync(id);
            if (prof == null) return NotFound();
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(prof);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Professor professor)
        {
            if (id != professor.ProfessorId) return NotFound();
            ModelState.Remove("Department");
            ModelState.Remove("Courses");
            ModelState.Remove(nameof(professor.Email));
            ModelState.Remove(nameof(professor.UserId));

            if (ModelState.IsValid)
            {
                var existing = await _context.Professors.AsNoTracking().FirstOrDefaultAsync(p => p.ProfessorId == id);
                if (existing == null) return NotFound();

                professor.Email = existing.Email;
                professor.UserId = existing.UserId;

                _context.Update(professor);
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(existing.UserId);
                if (user != null)
                {
                    user.FullName = professor.FullName;
                    await _userManager.UpdateAsync(user);
                }

                TempData["Success"] = _ui["Flash_ProfessorUpdated"];
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(professor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professor = await _context.Professors.FindAsync(id);
            if (professor != null)
            {
                if (!string.IsNullOrEmpty(professor.UserId))
                {
                    var user = await _userManager.FindByIdAsync(professor.UserId);
                    if (user != null)
                        await _userManager.DeleteAsync(user);
                }
                _context.Professors.Remove(professor);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_ProfessorDeleted"];
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
