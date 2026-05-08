using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StudentsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Students.Include(s => s.Department).ToListAsync());

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Student student)
        {
            ModelState.Remove("Department"); ModelState.Remove("Enrollments"); ModelState.Remove("Grades");
            if (ModelState.IsValid)
            {
                student.UserId = Guid.NewGuid().ToString();
                _context.Add(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student created!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(student);
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
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.StudentId) return NotFound();
            ModelState.Remove("Department"); ModelState.Remove("Enrollments"); ModelState.Remove("Grades");
            if (ModelState.IsValid)
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student updated!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(student);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.Include(s => s.Department).FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null) return NotFound();
            return View(student);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Student deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
