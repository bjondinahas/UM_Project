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
    [Authorize]
    public class DocumentRequestsController : Controller
    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITranscriptPdfService _pdf;
        private readonly IAdminAuditService _audit;

        public DocumentRequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ITranscriptPdfService pdf,
            IAdminAuditService audit, IUiText ui)
        {
            _ui = ui;

            _context = context;
            _userManager = userManager;
            _pdf = pdf;
            _audit = audit;
        }

        [Authorize(Roles = RoleNames.Parent)]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var children = await _context.ParentGuardians
                .Include(p => p.Student)
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            var studentIds = children.Select(c => c.StudentId).ToList();
            var requests = await _context.DocumentRequests
                .Include(r => r.Student)
                .Where(r => studentIds.Contains(r.StudentId))
                .OrderByDescending(r => r.RequestedAtUtc)
                .ToListAsync();

            ViewBag.Children = children;
            return View(requests);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Parent)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestTranscript(int studentId, string? notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var linked = await _context.ParentGuardians
                .AnyAsync(p => p.UserId == user.Id && p.StudentId == studentId);
            if (!linked)
            {
                TempData["Error"] = _ui["Flash_OnlyLinkedChildren"];
                return RedirectToAction(nameof(Index));
            }

            if (await _context.DocumentRequests.AnyAsync(r =>
                r.StudentId == studentId &&
                r.RequestType == DocumentRequestTypes.Transcript &&
                r.Status == DocumentRequestStatuses.Pending))
            {
                TempData["Error"] = _ui["Flash_PendingTranscript"];
                return RedirectToAction(nameof(Index));
            }

            _context.DocumentRequests.Add(new DocumentRequest
            {
                RequestedByUserId = user.Id,
                StudentId = studentId,
                RequestType = DocumentRequestTypes.Transcript,
                Status = DocumentRequestStatuses.Pending,
                ParentNotes = notes
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = _ui["Flash_TranscriptSubmitted"];
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = RoleNames.AdminPanel)]
        public async Task<IActionResult> Admin()
        {
            var list = await _context.DocumentRequests
                .Include(r => r.Student).ThenInclude(s => s!.Department)
                .OrderByDescending(r => r.RequestedAtUtc)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.AdminPanel)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? adminNotes)
        {
            var req = await _context.DocumentRequests.FindAsync(id);
            if (req == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            req.Status = DocumentRequestStatuses.Approved;
            req.AdminNotes = adminNotes;
            req.ProcessedAtUtc = DateTime.UtcNow;
            req.ProcessedByUserId = user?.Id;
            await _context.SaveChangesAsync();
            TempData["Success"] = _ui["Flash_RequestApproved"];
            return RedirectToAction(nameof(Admin));
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.AdminPanel)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? adminNotes)
        {
            var req = await _context.DocumentRequests.FindAsync(id);
            if (req == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            req.Status = DocumentRequestStatuses.Rejected;
            req.AdminNotes = adminNotes;
            req.ProcessedAtUtc = DateTime.UtcNow;
            req.ProcessedByUserId = user?.Id;
            await _context.SaveChangesAsync();
            TempData["Success"] = _ui["Flash_RequestRejected"];
            return RedirectToAction(nameof(Admin));
        }

        [Authorize(Roles = RoleNames.AdminPanel)]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var req = await _context.DocumentRequests.FindAsync(id);
            if (req == null) return NotFound();
            if (req.Status != DocumentRequestStatuses.Approved && req.Status != DocumentRequestStatuses.Completed)
            {
                TempData["Error"] = _ui["Flash_ApproveBeforePdf"];
                return RedirectToAction(nameof(Admin));
            }

            var bytes = await _pdf.GenerateTranscriptAsync(req.StudentId);
            if (req.Status == DocumentRequestStatuses.Approved)
            {
                req.Status = DocumentRequestStatuses.Completed;
                await _context.SaveChangesAsync();
            }

            await _audit.LogAsync(HttpContext, AdminActions.TranscriptDownload, AuditEntityTypes.Transcript,
                id.ToString(), $"StudentId={req.StudentId}");

            var student = await _context.Students.FindAsync(req.StudentId);
            var fileName = $"Transcript_{student?.StudentNumber ?? req.StudentId.ToString()}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
    }
}
