using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class FinalizeTaskValidator : AbstractValidator<FinalizeTaskDto>
{
    public FinalizeTaskValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("Task name is required.")
            .MaximumLength(100).WithMessage("Task name must not exceed 100 characters.");

        RuleFor(x => x.TaskDescription)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.TaskDescription))
            .WithMessage("Task description must not exceed 500 characters.");

        RuleFor(x => x.BaseXp)
            .GreaterThan(0).WithMessage("Base XP must be greater than 0.");

        RuleFor(x => x.TaskTypeId)
            .GreaterThan(0).WithMessage("Task type ID must be greater than 0.");

        RuleFor(x => x.FocusMethodId)
            .GreaterThan(0).WithMessage("Focus method ID must be greater than 0.");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.");

        RuleFor(x => x.BreakTime)
            .GreaterThanOrEqualTo(0).WithMessage("Break time must be 0 or greater.");
    }
}