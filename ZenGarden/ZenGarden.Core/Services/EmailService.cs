using System.Net;
using System.Net.Mail;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services;

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient = new("smtp.gmail.com")
    {
        Port = 587,
        Credentials = new NetworkCredential("zengardensp25se111@gmail.com", "kmiultfbypxnqxqu"),
        EnableSsl = true
    };

    public Task SendOtpEmailAsync(string email, string otp)
    {
        return SendEmailAsync(email, "Your ZenGarden OTP Code",
            $"Your OTP code is: <b>{otp}</b>. It will expire in 5 minutes.");
    }

    private async Task SendEmailAsync(string email, string subject, string body)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress("zengardensp25se111@gmail.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        await _smtpClient.SendMailAsync(mailMessage);
    }
}