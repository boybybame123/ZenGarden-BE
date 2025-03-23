using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class CreateChallengeValidator : AbstractValidator<CreateChallengeDto>
{
    public CreateChallengeValidator()
    {
        RuleFor(x => x.ChallengeTypeId)
            .GreaterThan(0).WithMessage("Challenge type is required.");

        RuleFor(x => x.ChallengeName)
            .NotEmpty().WithMessage("Challenge name is required.")
            .MaximumLength(100).WithMessage("Challenge name must be at most 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.");

        RuleFor(x => x.Reward)
            .GreaterThanOrEqualTo(0).WithMessage("Reward must be a positive value.");

        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("Start date is required.")
            .Must(BeAValidDate).WithMessage("Start date is invalid.")
            .GreaterThanOrEqualTo(DateTime.UtcNow).WithMessage("Start date cannot be in the past.");

        RuleFor(x => x.EndDate)
            .Must((dto, endDate) => endDate == null || (dto.StartDate != null && endDate > dto.StartDate))
            .WithMessage("End date must be after the start date if provided.");

        RuleFor(x => x.Tasks)
            .NotNull().WithMessage("Tasks list is required.")
            .Must(tasks => tasks is not { Count: 0 })
            .WithMessage("Tasks list cannot be empty."); // Nếu có thì phải có ít nhất 1 task
    }

    private static bool BeAValidDate(DateTime? date)
    {
        return date.HasValue && date.Value != default;
    }
}