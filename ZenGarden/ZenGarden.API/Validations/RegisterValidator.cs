using FluentValidation;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator(IUserRepository userRepository)
    {
        var userRepository1 = userRepository;
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Either Email or Phone must be provided.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MustAsync(async (username, _) =>
                !await userRepository1.ExistsByUserNameAsync(username))
            .WithMessage("Username is already taken.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[\W]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).When(x => x.RoleId.HasValue)
            .WithMessage("RoleId must be greater than 0.");
    }
}