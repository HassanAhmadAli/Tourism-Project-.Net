using System.Net;
using System.Net.Mail;
using Tourism.Models;

namespace Tourism;

public class AuthService(IConfiguration configuration)
{
    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        string subject = "Confirm your email";
        string message =
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.";
        return SendEmailAsync(email, subject, message);
    }

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        string subject = "Reset your password";
        string message = $"Your password reset code is: <b>{resetCode}</b>";
        return SendEmailAsync(email, subject, message);
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        string subject = "Reset your password";
        string message = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
        return SendEmailAsync(email, subject, message);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string messageBody)
    {
        var smtpServer = configuration["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]!);
        var senderEmail = configuration["EmailSettings:SenderEmail"];
        var senderPassword = configuration["EmailSettings:SenderPassword"];

        var client = new SmtpClient(smtpServer, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(senderEmail, senderPassword),
        };

        var mailMessage = new MailMessage(from: senderEmail!, to: toEmail, subject, messageBody)
        {
            IsBodyHtml = true,
        };

        await client.SendMailAsync(mailMessage);
    }
}
