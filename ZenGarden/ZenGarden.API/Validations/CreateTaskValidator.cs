using FluentValidation;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator(IFocusMethodRepository focusMethodRepository)
    {
        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("TaskName is required.")
            .MaximumLength(100).WithMessage("TaskName must be at most 100 characters.");

        RuleFor(x => x.UserTreeId)
            .GreaterThan(0).WithMessage("UserTreeId is required and must be greater than 0.");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate).WithMessage("StartDate must be before EndDate.");

        RuleFor(x => x.WorkDuration)
            .MustAsync(async (dto, workDuration, _) => 
            {
                if (!workDuration.HasValue || !dto.FocusMethodId.HasValue) return true;
                var focusMethod = await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value);
                if (focusMethod == null) return true;
                return workDuration.Value >= focusMethod.MinDuration && workDuration.Value <= focusMethod.MaxDuration;
            }).WithMessage("WorkDuration must be within the allowed range of the selected FocusMethod.");

        RuleFor(x => x.BreakTime)
            .MustAsync(async (dto, breakTime, _) => 
            {
                if (!breakTime.HasValue || !dto.FocusMethodId.HasValue) return true;
                var focusMethod = await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value);
                if (focusMethod == null) return true;
                return breakTime.Value >= focusMethod.MinBreak && breakTime.Value <= focusMethod.MaxBreak;
            }).WithMessage("BreakTime must be within the allowed range of the selected FocusMethod.");
    }
}
