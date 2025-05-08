using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class UpdateTaskResultValidator : AbstractValidator<UpdateTaskResultDto>
{
    public UpdateTaskResultValidator()
    {
        RuleFor(x => x.TaskNote)
            .MaximumLength(300).When(x => !string.IsNullOrWhiteSpace(x.TaskNote))
            .WithMessage("TaskNote must be at most 300 characters.");

        RuleFor(x => x.TaskResult)
            .MaximumLength(300).When(x => !string.IsNullOrWhiteSpace(x.TaskResult))
            .WithMessage("TaskResult must be at most 300 characters.")
            .Must(uri => string.IsNullOrWhiteSpace(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("TaskResult must be a valid URL if provided.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.TaskNote) || !string.IsNullOrWhiteSpace(x.TaskResult) ||
                       x.TaskFile != null)
            .WithMessage("At least one of TaskNote, TaskResult, or TaskFile must be provided.");
    }
}