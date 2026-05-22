using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;

namespace UM_Project.Controllers
{
    [Authorize(Roles = RoleNames.AdminPanel)]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt" };
        private const long MaxFileBytes = 15 * 1024 * 1024;

        public CoursesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Professor)
                .Include(c => c.Documents)
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Professor)
                .Include(c => c.Documents)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null) return NotFound();
            return View(course);
        }

        public async Task<IActionResult> Create()
        {
            await LoadLookupsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, List<IFormFile>? documents)
        {
            ClearNavigationValidation();
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                return View(course);
            }

            _context.Add(course);
            await _context.SaveChangesAsync();

            if (documents != null && documents.Count > 0)
                await SaveDocumentsAsync(course.CourseId, documents);

            TempData["Success"] = "Course created.";
            return RedirectToAction(nameof(Details), new { id = course.CourseId });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var course = await _context.Courses
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null) return NotFound();
            await LoadLookupsAsync();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course, List<IFormFile>? documents)
        {
            if (id != course.CourseId) return NotFound();
            ClearNavigationValidation();

            if (!ModelState.IsValid)
            {
                course.Documents = await _context.CourseDocuments.Where(d => d.CourseId == id).ToListAsync();
                await LoadLookupsAsync();
                return View(course);
            }

            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
                if (documents != null && documents.Count > 0)
                    await SaveDocumentsAsync(id, documents);
                TempData["Success"] = "Course updated.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Courses.Any(c => c.CourseId == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int documentId, int courseId)
        {
            var doc = await _context.CourseDocuments.FindAsync(documentId);
            if (doc != null && doc.CourseId == courseId)
            {
                DeletePhysicalFile(doc.StoredPath);
                _context.CourseDocuments.Remove(doc);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Document removed.";
            }
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CourseId == id);
            if (course != null)
            {
                foreach (var doc in course.Documents)
                    DeletePhysicalFile(doc.StoredPath);
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task SaveDocumentsAsync(int courseId, List<IFormFile> files)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "courses", courseId.ToString());
            Directory.CreateDirectory(uploadDir);

            foreach (var file in files.Where(f => f.Length > 0))
            {
                if (file.Length > MaxFileBytes)
                {
                    TempData["Error"] = $"Skipped {file.FileName}: file too large (max 15 MB).";
                    continue;
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    TempData["Error"] = $"Skipped {file.FileName}: type not allowed.";
                    continue;
                }

                var storedName = $"{Guid.NewGuid():N}{ext}";
                var relativePath = Path.Combine("uploads", "courses", courseId.ToString(), storedName).Replace('\\', '/');
                var fullPath = Path.Combine(_env.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
                await using (var stream = new FileStream(fullPath, FileMode.Create))
                    await file.CopyToAsync(stream);

                _context.CourseDocuments.Add(new CourseDocument
                {
                    CourseId = courseId,
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    FileName = file.FileName,
                    StoredPath = relativePath,
                    UploadedAtUtc = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        private void DeletePhysicalFile(string storedPath)
        {
            var full = Path.Combine(_env.WebRootPath, storedPath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(full))
                System.IO.File.Delete(full);
        }

        private void ClearNavigationValidation()
        {
            ModelState.Remove("Department");
            ModelState.Remove("Professor");
            ModelState.Remove("Enrollments");
            ModelState.Remove("Grades");
            ModelState.Remove("Schedules");
            ModelState.Remove("Documents");
        }

        private async Task LoadLookupsAsync()
        {
            ViewBag.Departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Professors = await _context.Professors.OrderBy(p => p.FullName).ToListAsync();
        }
    }
}
