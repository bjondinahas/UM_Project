using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public class SuperAdminController : Controller
    {
        private readonly IUiText _ui;
        private readonly ApplicationDbContext _context;
        private readonly ISystemSettingsService _settings;

        public SuperAdminController(ApplicationDbContext context, ISystemSettingsService settings, IUiText ui)
        {
            _ui = ui;

            _context = context;
            _settings = settings;
        }

        public async Task<IActionResult> Hub()
        {
            ViewBag.Settings = await _settings.GetAcademicSettingsAsync();
            ViewBag.LessonSlotCount = await _context.LessonSlots.CountAsync(l => l.IsActive);
            ViewBag.UserCount = await _context.Users.CountAsync();
            return View();
        }

        public async Task<IActionResult> AcademicSettings()
        {
            return View(await _settings.GetAcademicSettingsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcademicSettings(AcademicSettingsDto model)
        {
            try
            {
                await _settings.SaveAcademicSettingsAsync(model);
                TempData["Success"] = _ui["Flash_SettingsSaved"];
                return RedirectToAction(nameof(Hub));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> LessonSlots()
        {
            var slots = await _context.LessonSlots.OrderBy(l => l.SlotNumber).ToListAsync();
            ViewBag.LessonsPerDay = (await _settings.GetAcademicSettingsAsync()).LessonsPerDay;
            return View(slots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLessonSlot(int? lessonSlotId, int slotNumber, string title, string startTime, string endTime, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime))
            {
                TempData["Error"] = _ui["Flash_TitleTimesRequired"];
                return RedirectToAction(nameof(LessonSlots));
            }

            if (lessonSlotId > 0)
            {
                var slot = await _context.LessonSlots.FindAsync(lessonSlotId);
                if (slot == null) return NotFound();
                slot.SlotNumber = slotNumber;
                slot.Title = title.Trim();
                slot.StartTime = startTime.Trim();
                slot.EndTime = endTime.Trim();
                slot.IsActive = isActive;
            }
            else
            {
                if (await _context.LessonSlots.AnyAsync(l => l.SlotNumber == slotNumber))
                {
                    TempData["Error"] = _ui.Format("Flash_SlotExists", slotNumber);
                    return RedirectToAction(nameof(LessonSlots));
                }
                _context.LessonSlots.Add(new LessonSlot
                {
                    SlotNumber = slotNumber,
                    Title = title.Trim(),
                    StartTime = startTime.Trim(),
                    EndTime = endTime.Trim(),
                    IsActive = isActive
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = _ui["Flash_LessonSlotSaved"];
            return RedirectToAction(nameof(LessonSlots));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLessonSlot(int id)
        {
            var slot = await _context.LessonSlots.FindAsync(id);
            if (slot != null)
            {
                var inUse = await _context.AttendanceRecords.AnyAsync(a => a.LessonSlotId == id);
                if (inUse)
                {
                    slot.IsActive = false;
                    TempData["Success"] = _ui["Flash_LessonSlotInactive"];
                }
                else
                {
                    _context.LessonSlots.Remove(slot);
                    TempData["Success"] = _ui["Flash_LessonSlotRemoved"];
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(LessonSlots));
        }
    }
}
