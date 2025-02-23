using FluentValidation;
using ZenGarden.API.Models;

namespace ZenGarden.API.Validations
{
    public class LoginModelValidator : AbstractValidator<LoginModel>
    {
        public LoginModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email or phone is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .Unless(x => !string.IsNullOrEmpty(x.Phone)); 

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Email or phone is required.")
                .Matches(@"^\d{10,15}$").WithMessage("Invalid phone number format.")
                .Unless(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}