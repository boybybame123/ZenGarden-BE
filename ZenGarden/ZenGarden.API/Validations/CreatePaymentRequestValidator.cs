using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.WalletId)
            .GreaterThan(0).WithMessage("Wallet ID must be greater than 0.");

        RuleFor(x => x.PackageId)
            .GreaterThan(0).WithMessage("Package ID must be greater than 0.");
    }
}