namespace UM_Project.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendWelcomeEmailAsync(string to, string username, string password, string role);
        Task<bool> SendGradeNotificationAsync(string to, string studentName, string courseName, int grade);
    }
}
