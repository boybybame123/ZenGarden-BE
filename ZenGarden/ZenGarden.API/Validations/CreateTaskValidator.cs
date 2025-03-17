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

        RuleFor(x => x.TaskDescription)
            .MaximumLength(500).WithMessage("TaskDescription must be at most 500 characters.");

        RuleFor(x => x.TaskTypeId)
            .GreaterThan(0).WithMessage("TaskTypeId is required and must be greater than 0.");

        RuleFor(x => x.UserTreeId)
            .GreaterThan(0).WithMessage("UserTreeId is required and must be greater than 0.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required.")
            .GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate.");

        RuleFor(x => x.TotalDuration)
            .GreaterThan(0).When(x => x.TotalDuration.HasValue)
            .WithMessage("TotalDuration must be greater than 0 if provided.");

        RuleFor(x => x.WorkDuration)
            .MustAsync(async (dto, workDuration, _) =>
            {
                if (!workDuration.HasValue || !dto.FocusMethodId.HasValue) return true;
                var focusMethod = await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value);
                if (focusMethod == null) return false;
                return workDuration.Value >= focusMethod.MinDuration && workDuration.Value <= focusMethod.MaxDuration;
            }).WithMessage("WorkDuration must be within the allowed range of the selected FocusMethod.");

        RuleFor(x => x.BreakTime)
            .MustAsync(async (dto, breakTime, _) =>
            {
                if (!breakTime.HasValue || !dto.FocusMethodId.HasValue) return true;
                var focusMethod = await focusMethodRepository.GetByIdAsync(dto.FocusMethodId.Value);
                if (focusMethod == null) return false; 
                return breakTime.Value >= focusMethod.MinBreak && breakTime.Value <= focusMethod.MaxBreak;
            }).WithMessage("BreakTime must be within the allowed range of the selected FocusMethod.");

        RuleFor(x => x.FocusMethodId)
            .MustAsync(async (focusMethodId, _) =>
            {
                if (!focusMethodId.HasValue) return true; 
                var focusMethod = await focusMethodRepository.GetByIdAsync(focusMethodId.Value);
                return focusMethod != null;
            }).WithMessage("The selected FocusMethodId does not exist.");


    }
}
