using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("Task name is required.")
            .MaximumLength(100).WithMessage("Task name must not exceed 100 characters.");

        RuleFor(x => x.TaskDescription)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.TaskDescription))
            .WithMessage("Task description must not exceed 500 characters.");

        RuleFor(x => x.TaskTypeId)
            .GreaterThan(0).WithMessage("Task type ID must be greater than 0.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.")
            .Must((dto, endDate) =>
                !dto.TotalDuration.HasValue ||
                endDate >= dto.StartDate.AddMinutes(dto.TotalDuration.Value))
            .WithMessage(x => $"End date must be at least {x.TotalDuration} minutes after start date.");

        RuleFor(x => x.TotalDuration)
            .GreaterThan(0).WithMessage("Total duration must be greater than 0.");

        RuleFor(x => x.WorkDuration)
            .GreaterThan(0).When(x => x.WorkDuration.HasValue)
            .WithMessage("Work duration must be greater than 0.");

        RuleFor(x => x.BreakTime)
            .GreaterThan(0).When(x => x.BreakTime.HasValue)
            .WithMessage("Break time must be greater than 0.");

        RuleFor(x => x.UserTreeId)
            .GreaterThan(0).When(x => x.UserTreeId.HasValue && x.TaskTypeId != 4)
            .WithMessage("User tree ID must be greater than 0.");

        RuleFor(x => x.FocusMethodId)
            .GreaterThan(0).When(x => x.FocusMethodId.HasValue)
            .WithMessage("Focus method ID must be greater than 0.");
    }
}