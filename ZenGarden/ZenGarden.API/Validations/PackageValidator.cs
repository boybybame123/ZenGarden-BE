using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class PackageValidator : AbstractValidator<PackageDto>
{
    public PackageValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Package name is required.")
            .MaximumLength(100).WithMessage("Package name must not exceed 100 characters.")
            .Matches("^[a-zA-Z0-9\\s-]+$")
            .WithMessage("Package name can only contain letters, numbers, spaces and hyphens.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
    }
}