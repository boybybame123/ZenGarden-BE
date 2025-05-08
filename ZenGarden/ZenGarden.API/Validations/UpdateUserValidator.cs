using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class UpdateUserValidator : AbstractValidator<UpdateUserDTO>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.UserName)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Username must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9_.-]+$")
            .When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Username can only contain letters, numbers, and the following special characters: ., -, _");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Phone)
            .Matches(@"^\d{10,15}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone number must be between 10 and 15 digits.");
    }
}