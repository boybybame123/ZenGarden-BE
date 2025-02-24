using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Either Email or Phone must be provided.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Phone)
                .Matches(@"^\d{10,15}$").WithMessage("Phone number must be between 10 and 15 digits.")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}