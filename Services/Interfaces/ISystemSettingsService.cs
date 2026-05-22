using UM_Project.Models;

namespace UM_Project.Services.Interfaces
{
    public interface ISystemSettingsService
    {
        Task<AcademicSettingsDto> GetAcademicSettingsAsync();
        Task SaveAcademicSettingsAsync(AcademicSettingsDto settings);
        Task<int> GetIntAsync(string key, int defaultValue);
        Task<bool> IsValidGradeAsync(int value);
        Task<bool> IsPassingGradeAsync(int value);
    }
}
