using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class UpdateTaskSimpleValidator : AbstractValidator<UpdateTaskSimpleDto>
{
    public UpdateTaskSimpleValidator()
    {
        // Only validate TotalDuration
        RuleFor(x => x.TotalDuration)
            .GreaterThan(0)
            .WithMessage("TotalDuration must be greater than 0.");
    }
}