using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendOtpEmailAsync(string email, string otp)
    {
        await SendEmailAsync(email, "Your ZenGarden OTP Code", 
            $"Your OTP code is: <b>{otp}</b>. It will expire in 5 minutes.");
    }

    private async Task SendEmailAsync(string email, string subject, string body)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("ZenGarden", configuration["EmailSettings:Username"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(configuration["EmailSettings:SmtpServer"], int.Parse(configuration["EmailSettings:Port"] ?? string.Empty), false);
        await smtpClient.AuthenticateAsync(configuration["EmailSettings:Username"], configuration["EmailSettings:Password"]);
        await smtpClient.SendAsync(emailMessage);
        await smtpClient.DisconnectAsync(true);
    }
}