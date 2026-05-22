using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class AccountProvisioningService : IAccountProvisioningService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AccountProvisioningSettings _settings;

        private const string StudentEmailPrefix = "stu";
        private const string ProfessorEmailPrefix = "prof";
        private const string ParentEmailPrefix = "par";

        public AccountProvisioningService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOptions<AccountProvisioningSettings> settings)
        {
            _context = context;
            _userManager = userManager;
            _settings = settings.Value;
        }

        public async Task<string> GenerateStudentNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"{_settings.StudentNumberPrefix}{year}";
            var numbers = await _context.Students.Select(s => s.StudentNumber).ToListAsync();
            return $"{prefix}{(GetMaxSequence(numbers, prefix) + 1):D4}";
        }

        public async Task<string> GenerateProfessorIdAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"{_settings.ProfessorIdPrefix}{year}";
            var ids = await _context.Users.Select(u => u.CustomId).ToListAsync();
            return $"{prefix}{(GetMaxSequence(ids, prefix) + 1):D4}";
        }

        private static int GetMaxSequence(IEnumerable<string> values, string prefix)
        {
            var maxSeq = 0;
            foreach (var value in values)
            {
                if (string.IsNullOrEmpty(value) || !value.StartsWith(prefix, StringComparison.Ordinal))
                    continue;
                if (value.Length > prefix.Length &&
                    int.TryParse(value.AsSpan(prefix.Length), out var seq) &&
                    seq > maxSeq)
                    maxSeq = seq;
            }
            return maxSeq;
        }

        public async Task<string> GenerateUniqueEmailAsync(string fullName, string rolePrefix)
        {
            var domain = _settings.EmailDomain.Trim().ToLowerInvariant();
            var baseLocal = BuildRoleScopedLocalPart(fullName, rolePrefix);
            var existingEmails = await GetAllKnownEmailsAsync();

            for (var n = 1; n <= 9999; n++)
            {
                var email = $"{baseLocal}{n:D3}@{domain}";
                if (!existingEmails.Contains(email))
                    return email;
            }

            throw new InvalidOperationException("Could not generate a unique email address.");
        }

        private async Task<HashSet<string>> GetAllKnownEmailsAsync()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in await _context.Users.Select(u => u.Email).ToListAsync())
                if (!string.IsNullOrEmpty(e)) set.Add(e);
            foreach (var e in await _context.Students.Select(s => s.Email).ToListAsync())
                set.Add(e);
            foreach (var e in await _context.Professors.Select(p => p.Email).ToListAsync())
                set.Add(e);
            foreach (var e in await _context.ParentGuardians.Select(p => p.Email).ToListAsync())
                set.Add(e);
            return set;
        }

        /// <summary>stu.john.smith, prof.jane.doe, par.mary.smith — role prefix avoids collisions across roles.</summary>
        internal static string BuildRoleScopedLocalPart(string fullName, string rolePrefix)
        {
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return $"{rolePrefix}.user";

            if (parts.Length == 1)
                return $"{rolePrefix}.{SanitizeLocalPart(parts[0])}";

            var first = SanitizeLocalPart(parts[0]);
            var last = SanitizeLocalPart(parts[^1]);
            return $"{rolePrefix}.{first}.{last}";
        }

        public async Task<ProvisionedAccountResult> ProvisionStudentAsync(string fullName, int departmentId)
        {
            var studentNumber = await GenerateStudentNumberAsync();
            var email = await GenerateUniqueEmailAsync(fullName, StudentEmailPrefix);
            return await CreateLinkedUserAsync(fullName, email, studentNumber, RoleNames.Student,
                async userId =>
                {
                    _context.Students.Add(new Student
                    {
                        FullName = fullName.Trim(),
                        Email = email,
                        StudentNumber = studentNumber,
                        DepartmentId = departmentId,
                        UserId = userId
                    });
                    await _context.SaveChangesAsync();
                });
        }

        public async Task<ProvisionedAccountResult> ProvisionProfessorAsync(string fullName, int departmentId, string? title = null)
        {
            var staffId = await GenerateProfessorIdAsync();
            var email = await GenerateUniqueEmailAsync(fullName, ProfessorEmailPrefix);
            return await CreateLinkedUserAsync(fullName, email, staffId, RoleNames.Professor,
                async userId =>
                {
                    _context.Professors.Add(new Professor
                    {
                        FullName = fullName.Trim(),
                        Email = email,
                        Title = string.IsNullOrWhiteSpace(title) ? "Professor" : title.Trim(),
                        DepartmentId = departmentId,
                        UserId = userId
                    });
                    await _context.SaveChangesAsync();
                });
        }

        public async Task<ProvisionedAccountResult> LinkOrCreateParentAsync(string fullName, string? email, int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                return new ProvisionedAccountResult { Success = false, Error = "Student not found." };

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim().ToLowerInvariant();
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                    return await LinkExistingParentUserAsync(existingUser, fullName, email, studentId);
            }

            var generatedEmail = await GenerateUniqueEmailAsync(fullName, ParentEmailPrefix);
            var parentCode = $"PAR{DateTime.UtcNow.Year}{(await _context.ParentGuardians.CountAsync() + 1):D4}";

            return await CreateLinkedUserAsync(fullName, generatedEmail, parentCode, RoleNames.Parent,
                async userId =>
                {
                    await AddParentGuardianRowAsync(userId, fullName, generatedEmail, studentId);
                });
        }

        public async Task<ProvisionedAccountResult> AssignChildToExistingParentAsync(int parentGuardianId, int studentId)
        {
            var anchor = await _context.ParentGuardians.FindAsync(parentGuardianId);
            if (anchor == null || string.IsNullOrEmpty(anchor.UserId))
                return new ProvisionedAccountResult { Success = false, Error = "Parent not found." };

            if (await _context.ParentGuardians.AnyAsync(p => p.UserId == anchor.UserId && p.StudentId == studentId))
                return new ProvisionedAccountResult { Success = false, Error = "This child is already linked to this parent." };

            await AddParentGuardianRowAsync(anchor.UserId, anchor.FullName, anchor.Email, studentId);
            return new ProvisionedAccountResult
            {
                Success = true,
                Email = anchor.Email,
                GeneratedId = anchor.Email,
                UserId = anchor.UserId
            };
        }

        private async Task<ProvisionedAccountResult> LinkExistingParentUserAsync(
            ApplicationUser user, string fullName, string email, int studentId)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(RoleNames.Parent))
            {
                return new ProvisionedAccountResult
                {
                    Success = false,
                    Error = $"Email {email} belongs to a {roles.FirstOrDefault() ?? "non-parent"} account. Use a different email."
                };
            }

            if (await _context.ParentGuardians.AnyAsync(p => p.UserId == user.Id && p.StudentId == studentId))
                return new ProvisionedAccountResult { Success = false, Error = "Parent is already linked to this student." };

            await AddParentGuardianRowAsync(user.Id, fullName.Trim(), email, studentId);
            return new ProvisionedAccountResult
            {
                Success = true,
                Email = email,
                GeneratedId = user.CustomId,
                UserId = user.Id
            };
        }

        private async Task AddParentGuardianRowAsync(string userId, string fullName, string email, int studentId)
        {
            _context.ParentGuardians.Add(new ParentGuardian
            {
                FullName = fullName.Trim(),
                Email = email,
                UserId = userId,
                StudentId = studentId
            });
            await _context.SaveChangesAsync();
        }

        private async Task<ProvisionedAccountResult> CreateLinkedUserAsync(
            string fullName, string email, string customId, string role, Func<string, Task> saveProfile)
        {
            if (await _userManager.FindByEmailAsync(email) != null)
                return new ProvisionedAccountResult { Success = false, Error = $"Email {email} is already in use." };

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName.Trim(),
                CustomId = customId,
                Address = "Shkolla Fillore \"Drita\", Prishtinë",
                MustChangePassword = true
            };

            var createResult = await _userManager.CreateAsync(user, _settings.DefaultPassword);
            if (!createResult.Succeeded)
                return new ProvisionedAccountResult { Success = false, Error = string.Join(" ", createResult.Errors.Select(e => e.Description)) };

            await _userManager.AddToRoleAsync(user, role);

            try { await saveProfile(user.Id); }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            return new ProvisionedAccountResult
            {
                Success = true,
                Email = email,
                GeneratedId = customId,
                TemporaryPassword = _settings.DefaultPassword,
                UserId = user.Id
            };
        }

        private static string SanitizeLocalPart(string value)
        {
            var chars = value.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c)).ToArray();
            return chars.Length > 0 ? new string(chars) : "user";
        }
    }
}
