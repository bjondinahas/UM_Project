using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUiText _ui;

        public DepartmentsController(ApplicationDbContext context, IUiText ui)
        {
            _context = context;
            _ui = ui;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments.ToListAsync();
            // Debug: shiko në terminal sa departamente ka
            System.Diagnostics.Debug.WriteLine($"Departments count: {departments.Count}");
            return View(departments);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_DepartmentCreated"];
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_DepartmentUpdated"];
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();
            return View(dept);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = _ui["Flash_DepartmentDeleted"];
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
