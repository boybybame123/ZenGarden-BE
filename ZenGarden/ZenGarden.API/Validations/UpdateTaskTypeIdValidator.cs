using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class UpdateTaskTypeIdValidator : AbstractValidator<UpdateTaskTypeIdDto>
{
    public UpdateTaskTypeIdValidator()
    {
        RuleFor(x => x.NewTaskTypeId)
            .GreaterThan(0).WithMessage("NewTaskTypeId must be greater than 0.");

        RuleFor(x => x.NewDuration)
            .GreaterThan(0).WithMessage("NewDuration must be greater than 0.")
            .Must((dto, duration) =>
            {
                // Validate duration based on a task type
                return dto.NewTaskTypeId switch
                {
                    2 => duration >= 30, // Weekly task: >= 30 minutes
                    3 => duration >= 180, // Monthly task: >= 180 minutes
                    4 => duration >= 30, // Challenge task: >= 30 minutes
                    _ => false
                };
            })
            .WithMessage("Invalid duration for the selected task type.");
    }
}