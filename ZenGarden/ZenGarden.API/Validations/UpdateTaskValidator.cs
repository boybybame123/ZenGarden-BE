using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskDto>
{
    // public UpdateTaskValidator()
    // {
    //     RuleFor(x => x.TaskName)
    //         .NotEmpty().WithMessage("TaskName is required.")
    //         .MaximumLength(100).WithMessage("TaskName must be at most 100 characters.");
    //
    //     RuleFor(x => x.TaskDescription)
    //         .MaximumLength(500).WithMessage("TaskDescription must be at most 500 characters.");
    //
    //     RuleFor(x => x.TaskNote)
    //         .MaximumLength(300).WithMessage("TaskNote must be at most 300 characters.");
    //
    //     RuleFor(x => x.TaskResult)
    //         .MaximumLength(300).WithMessage("TaskResult must be at most 300 characters.")
    //         .Must(uri => string.IsNullOrWhiteSpace(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
    //         .WithMessage("TaskResult must be a valid URL if provided.");
    //
    //     RuleFor(x => x.TotalDuration)
    //         .GreaterThan(0).When(x => x.TotalDuration.HasValue)
    //         .WithMessage("TotalDuration must be greater than 0 if provided.");
    //
    //     RuleFor(x => x.WorkDuration)
    //         .GreaterThanOrEqualTo(0).When(x => x.WorkDuration.HasValue)
    //         .WithMessage("WorkDuration must be non-negative.");
    //
    //     RuleFor(x => x.BreakTime)
    //         .GreaterThanOrEqualTo(0).When(x => x.BreakTime.HasValue)
    //         .WithMessage("BreakTime must be non-negative.");
    //
    //     RuleFor(x => x)
    //         .Must(x => !x.TotalDuration.HasValue ||
    //                    !x.WorkDuration.HasValue ||
    //                    !x.BreakTime.HasValue ||
    //                    x.WorkDuration + x.BreakTime <= x.TotalDuration)
    //         .WithMessage("WorkDuration + BreakTime must not exceed TotalDuration.");
    //
    //     RuleFor(x => x.FocusMethodId)
    //         .GreaterThan(0).When(x => x.FocusMethodId.HasValue)
    //         .WithMessage("FocusMethodId must be greater than 0 if provided.");
    //
    //     RuleFor(x => x.StartDate)
    //         .LessThan(x => x.EndDate)
    //         .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
    //         .WithMessage("StartDate must be before EndDate.");
    // }
}