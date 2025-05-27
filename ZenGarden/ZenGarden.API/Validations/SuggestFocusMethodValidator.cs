using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class SuggestFocusMethodValidator : AbstractValidator<SuggestFocusMethodDto>
{
    public SuggestFocusMethodValidator()
    {
        RuleFor(x => x.TaskName)
            .MaximumLength(100).WithMessage("TaskName must be at most 100 characters.");

        RuleFor(x => x.TaskDescription)
            .MaximumLength(500).WithMessage("TaskDescription must be at most 500 characters.");

        RuleFor(x => x.TotalDuration)
            .GreaterThan(0).WithMessage("TotalDuration must be greater than 0.")
            .When(x => x.TotalDuration.HasValue);

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("StartDate must be before EndDate.");
        RuleFor(x => x)
            .Must(x =>
            {
                if (!x.TotalDuration.HasValue) return true;
                var availableMinutes = (x.EndDate - x.StartDate).TotalMinutes;
                return x.TotalDuration.Value <= availableMinutes;
            })
            .WithMessage("TotalDuration must not exceed the time between StartDate and EndDate.");
    }
}