using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0).WithMessage("TaskId must be greater than 0.");

        RuleFor(x => x.TaskName)
            .MaximumLength(100).WithMessage("TaskName must be at most 100 characters.");

        RuleFor(x => x.TotalDuration)
            .GreaterThan(0).WithMessage("TotalDuration must be greater than 0.");

        RuleFor(x => x.WorkDuration)
            .GreaterThanOrEqualTo(0).WithMessage("WorkDuration must be non-negative.");

        RuleFor(x => x.BreakTime)
            .GreaterThanOrEqualTo(0).WithMessage("BreakTime must be non-negative.");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("StartDate must be before EndDate.");
    }
}