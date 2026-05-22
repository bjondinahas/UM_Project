using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly ApplicationDbContext _context;

        public SystemSettingsService(ApplicationDbContext context) => _context = context;

        public async Task<AcademicSettingsDto> GetAcademicSettingsAsync() => new()
        {
            GradeMinimum = await GetIntAsync(SystemSettingKeys.GradeMinimum, 1),
            GradeMaximum = await GetIntAsync(SystemSettingKeys.GradeMaximum, 5),
            GradePassingMinimum = await GetIntAsync(SystemSettingKeys.GradePassingMinimum, 3),
            LessonsPerDay = await GetIntAsync(SystemSettingKeys.LessonsPerDay, 6)
        };

        public async Task SaveAcademicSettingsAsync(AcademicSettingsDto settings)
        {
            if (settings.GradeMinimum >= settings.GradeMaximum)
                throw new InvalidOperationException("Minimum grade must be less than maximum.");
            if (settings.GradePassingMinimum < settings.GradeMinimum || settings.GradePassingMinimum > settings.GradeMaximum)
                throw new InvalidOperationException("Passing grade must be between minimum and maximum.");
            if (settings.LessonsPerDay < 1 || settings.LessonsPerDay > 12)
                throw new InvalidOperationException("Lessons per day must be between 1 and 12.");

            await SetAsync(SystemSettingKeys.GradeMinimum, settings.GradeMinimum.ToString(), "Lowest allowed grade value");
            await SetAsync(SystemSettingKeys.GradeMaximum, settings.GradeMaximum.ToString(), "Highest allowed grade value");
            await SetAsync(SystemSettingKeys.GradePassingMinimum, settings.GradePassingMinimum.ToString(), "Minimum grade to pass");
            await SetAsync(SystemSettingKeys.LessonsPerDay, settings.LessonsPerDay.ToString(), "Lesson slots per school day");
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetIntAsync(string key, int defaultValue)
        {
            var row = await _context.SystemSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == key);
            return row != null && int.TryParse(row.SettingValue, out var v) ? v : defaultValue;
        }

        public async Task<bool> IsValidGradeAsync(int value)
        {
            var s = await GetAcademicSettingsAsync();
            return value >= s.GradeMinimum && value <= s.GradeMaximum;
        }

        public async Task<bool> IsPassingGradeAsync(int value)
        {
            var s = await GetAcademicSettingsAsync();
            return value >= s.GradePassingMinimum;
        }

        public async Task ApplyGradeScaleOneToFiveAsync()
        {
            await SetAsync(SystemSettingKeys.GradeMinimum, "1", "Lowest allowed grade value");
            await SetAsync(SystemSettingKeys.GradeMaximum, "5", "Highest allowed grade value");
            await SetAsync(SystemSettingKeys.GradePassingMinimum, "3", "Minimum grade to pass");
            await _context.SaveChangesAsync();
        }

        public async Task MigrateLegacyGradesToCurrentScaleAsync()
        {
            var s = await GetAcademicSettingsAsync();
            var grades = await _context.Grades.ToListAsync();
            var changed = false;
            foreach (var g in grades)
            {
                if (g.Value > s.GradeMaximum)
                {
                    g.Value = Math.Clamp(g.Value - 5, s.GradeMinimum, s.GradeMaximum);
                    changed = true;
                }
                else if (g.Value < s.GradeMinimum)
                {
                    g.Value = s.GradeMinimum;
                    changed = true;
                }
            }
            if (changed)
                await _context.SaveChangesAsync();
        }

        private async Task SetAsync(string key, string value, string? description)
        {
            var row = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (row == null)
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    Description = description
                });
            }
            else
            {
                row.SettingValue = value;
                if (description != null) row.Description = description;
            }
        }
    }
}
