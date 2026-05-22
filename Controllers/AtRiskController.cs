using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class AtRiskController : Controller
    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;
        private readonly IAtRiskService _atRisk;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminAuditService _audit;

        public AtRiskController(ApplicationDbContext context, IAtRiskService atRisk, UserManager<ApplicationUser> userManager, IAdminAuditService audit, IUiText ui)
        {
            _ui = ui;

            _context = context;
            _atRisk = atRisk;
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _atRisk.GetAtRiskStudentsAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int studentId, string note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                TempData["Error"] = _ui["Flash_NoteRequired"];
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            _context.InterventionNotes.Add(new InterventionNote
            {
                StudentId = studentId,
                Note = note.Trim(),
                CreatedByUserId = user?.Id ?? ""
            });
            await _context.SaveChangesAsync();
            await _audit.LogAsync(HttpContext, AdminActions.AtRiskNote, AuditEntityTypes.InterventionNote,
                studentId.ToString(), note.Trim().Length > 200 ? note.Trim()[..200] : note.Trim());
            TempData["Success"] = _ui["Flash_InterventionSaved"];
            return RedirectToAction(nameof(Index));
        }
    }
}
