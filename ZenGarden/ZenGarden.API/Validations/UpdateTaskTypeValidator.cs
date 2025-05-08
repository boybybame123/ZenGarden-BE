using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class UpdateTaskTypeValidator : AbstractValidator<UpdateTaskTypeDto>
{
    public UpdateTaskTypeValidator()
    {
        RuleFor(x => x.TaskTypeName)
            .NotEmpty().WithMessage("TaskTypeName is required.")
            .MaximumLength(100).WithMessage("TaskTypeName must be at most 100 characters.")
            .Matches("^[a-zA-Z0-9\\s-]+$")
            .WithMessage("TaskTypeName can only contain letters, numbers, spaces and hyphens.");

        RuleFor(x => x.TaskTypeDescription)
            .MaximumLength(255).WithMessage("TaskTypeDescription must be at most 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.TaskTypeDescription));
    }
}