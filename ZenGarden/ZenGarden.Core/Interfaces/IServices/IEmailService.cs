namespace ZenGarden.Core.Interfaces.IServices;

public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otp);
}