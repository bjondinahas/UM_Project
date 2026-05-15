using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using UM_Project.Models;
using UM_Project.Services.Interfaces;

namespace UM_Project.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("UM_Project", _emailSettings.SenderEmail));
                email.To.Add(new MailboxAddress("", to));
                email.Subject = subject;
                email.Body = new TextPart("html") { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string to, string username, string password, string role)
        {
            var subject = "Welcome to UM_Project!";
            var body = $@"
            <html>
            <head><style>body{{font-family:Arial;}} .container{{max-width:600px;margin:0 auto;padding:20px;}} .header{{background:#1F4959;color:white;padding:15px;text-align:center;}} .content{{padding:20px;}} .credentials{{background:#f0f2f5;padding:15px;border-radius:8px;}}</style></head>
            <body>
            <div class='container'>
                <div class='header'><h2>Welcome to UM_Project</h2></div>
                <div class='content'>
                    <p>Hello {username},</p>
                    <p>Your account has been created with the following credentials:</p>
                    <div class='credentials'>
                        <p><strong>Email:</strong> {to}</p>
                        <p><strong>Password:</strong> {password}</p>
                        <p><strong>Role:</strong> {role}</p>
                    </div>
                    <p>Please login and change your password after first access.</p>
                </div>
            </div>
            </body>
            </html>";
            
            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendGradeNotificationAsync(string to, string studentName, string courseName, int grade)
        {
            var subject = $"Grade Assigned: {courseName}";
            var body = $@"
            <html>
            <head><style>body{{font-family:Arial;}} .container{{max-width:600px;margin:0 auto;padding:20px;}} .header{{background:#1F4959;color:white;padding:15px;text-align:center;}} .grade{{font-size:24px;font-weight:bold;color:#2ecc71;}}</style></head>
            <body>
            <div class='container'>
                <div class='header'><h2>Grade Notification</h2></div>
                <div class='content'>
                    <p>Dear {studentName},</p>
                    <p>Your grade for the course <strong>{courseName}</strong> has been assigned.</p>
                    <p class='grade'>Grade: {grade} / 10</p>
                    <p>Login to the system to see more details.</p>
                </div>
            </div>
            </body>
            </html>";
            
            return await SendEmailAsync(to, subject, body);
        }
    }
}
