using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class UpdateChallengeValidator : AbstractValidator<UpdateChallengeDto>
{
    public UpdateChallengeValidator()
    {
        RuleFor(x => x.ChallengeTypeId)
            .GreaterThan(0).When(x => x.ChallengeTypeId.HasValue)
            .WithMessage("Challenge type must be greater than 0 if provided.");

        RuleFor(x => x.ChallengeName)
            .NotEmpty().When(x => x.ChallengeName != null)
            .WithMessage("Challenge name is required if provided.")
            .MaximumLength(100).When(x => x.ChallengeName != null)
            .WithMessage("Challenge name must be at most 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().When(x => x.Description != null)
            .WithMessage("Description is required if provided.")
            .MaximumLength(500).When(x => x.Description != null)
            .WithMessage("Description must be at most 500 characters.");

        RuleFor(x => x.Reward)
            .GreaterThanOrEqualTo(0).When(x => x.Reward.HasValue)
            .WithMessage("Reward must be a positive value if provided.");

        RuleFor(x => x.MaxParticipants)
            .GreaterThan(0).When(x => x.MaxParticipants.HasValue)
            .WithMessage("Max participants must be greater than 0 if provided.");

        RuleFor(x => x.StartDate)
            .Must(BeAValidDate).When(x => x.StartDate.HasValue)
            .WithMessage("Start date is invalid if provided.");

        RuleFor(x => x.EndDate)
            .Must((dto, endDate) => !endDate.HasValue || !dto.StartDate.HasValue || endDate > dto.StartDate)
            .WithMessage("End date must be after the start date if both are provided.");
    }

    private static bool BeAValidDate(DateTime? date)
    {
        return date.HasValue && date.Value != default;
    }
}