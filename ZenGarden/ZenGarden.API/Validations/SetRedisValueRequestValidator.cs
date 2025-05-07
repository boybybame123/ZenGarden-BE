using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class SetRedisValueRequestValidator : AbstractValidator<SetRedisValueRequest>
{
    public SetRedisValueRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(255).WithMessage("Key must not exceed 255 characters.")
            .Matches("^[a-zA-Z0-9_:.-]+$").WithMessage("Key can only contain letters, numbers, and the following special characters: :, ., -, _");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required.");

        RuleFor(x => x.ExpiryInSeconds)
            .GreaterThan(0).When(x => x.ExpiryInSeconds.HasValue)
            .WithMessage("Expiry time must be greater than 0 if provided.");
    }
} 