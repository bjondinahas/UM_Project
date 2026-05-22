using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;

namespace UM_Project.Helpers
{
    public static class StudentSelectListHelper
    {
        public static string FormatLabel(Student s) =>
            $"{s.StudentNumber} — {s.FullName} — {s.Department?.DepartmentName ?? "No dept"}";

        public static async Task<SelectList> BuildAsync(
            ApplicationDbContext context,
            int? selectedStudentId = null)
        {
            var students = await context.Students
                .Include(s => s.Department)
                .OrderBy(s => s.FullName)
                .ThenBy(s => s.StudentNumber)
                .ToListAsync();

            return new SelectList(
                students.Select(s => new { s.StudentId, Label = FormatLabel(s) }),
                "StudentId",
                "Label",
                selectedStudentId);
        }

        public static async Task<SelectList> BuildParentAccountSelectListAsync(
            ApplicationDbContext context,
            int? selectedParentGuardianId = null)
        {
            var rows = await context.ParentGuardians
                .Where(p => p.UserId != null && p.UserId != "")
                .OrderBy(p => p.FullName)
                .ThenBy(p => p.Email)
                .ToListAsync();

            var distinct = rows
                .GroupBy(p => p.UserId)
                .Select(g =>
                {
                    var first = g.First();
                    var childCount = g.Count();
                    var label = childCount > 1
                        ? $"{first.FullName} ({first.Email}) — {childCount} children linked"
                        : $"{first.FullName} ({first.Email})";
                    return new { first.ParentId, Label = label };
                })
                .OrderBy(x => x.Label)
                .ToList();

            return new SelectList(distinct, "ParentId", "Label", selectedParentGuardianId);
        }
    }
}
