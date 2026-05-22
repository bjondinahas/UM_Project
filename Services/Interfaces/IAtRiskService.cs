using UM_Project.Models;

namespace UM_Project.Services.Interfaces
{
    public interface IAtRiskService
    {
        Task<List<AtRiskStudentViewModel>> GetAtRiskStudentsAsync();
    }
}
