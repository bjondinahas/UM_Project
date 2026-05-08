using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProfessorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProfessorsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Professors.Include(p => p.Department).ToListAsync());

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Professor professor)
        {
            ModelState.Remove("Department"); ModelState.Remove("Courses");
            if (ModelState.IsValid)
            {
                professor.UserId = Guid.NewGuid().ToString();
                _context.Add(professor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Professor created!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(professor);
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
        public async Task<IActionResult> Edit(int id, Professor professor)
        {
            if (id != professor.ProfessorId) return NotFound();
            ModelState.Remove("Department"); ModelState.Remove("Courses");
            if (ModelState.IsValid)
            {
                _context.Update(professor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Professor updated!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(professor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var prof = await _context.Professors.Include(p => p.Department).FirstOrDefaultAsync(m => m.ProfessorId == id);
            if (prof == null) return NotFound();
            return View(prof);
        }

    [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professor = await _context.Professors.FindAsync(id);
            if (professor != null)
            {
                _context.Professors.Remove(professor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Professor deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
