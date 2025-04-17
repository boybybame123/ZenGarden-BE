using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("TaskName is required.")
            .MaximumLength(100).WithMessage("TaskName must be at most 100 characters.");

        RuleFor(x => x.TaskDescription)
            .MaximumLength(500).WithMessage("TaskDescription must be at most 500 characters.");

        RuleFor(x => x.TaskTypeId)
            .GreaterThan(0).WithMessage("TaskTypeId must be greater than 0.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate.")
            .Must((dto, endDate) =>
                !dto.TotalDuration.HasValue || endDate >= dto.StartDate.AddMinutes(dto.TotalDuration.Value))
            .WithMessage("EndDate must be at least TotalDuration minutes after StartDate.");

        RuleFor(x => x.TotalDuration)
            .GreaterThan(0).WithMessage("TotalDuration must be greater than 0.")
            .When(x => x.TotalDuration.HasValue);

        RuleFor(x => x.WorkDuration)
            .GreaterThan(0).WithMessage("WorkDuration must be greater than 0.")
            .When(x => x.WorkDuration.HasValue);

        RuleFor(x => x.BreakTime)
            .GreaterThanOrEqualTo(0).WithMessage("BreakTime must be 0 or greater.")
            .When(x => x.BreakTime.HasValue);
    }
}