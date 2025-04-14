using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class ReorderTasksValidator : AbstractValidator<List<ReorderTaskDto>>
{
    public ReorderTasksValidator()
    {
        RuleFor(x => x)
            .Must(HaveUniquePriorities)
            .WithMessage("Duplicate priorities are not allowed.");

        RuleForEach(x => x)
            .ChildRules(task =>
            {
                task.RuleFor(t => t.Priority)
                    .InclusiveBetween(1, 20)
                    .WithMessage("Priority must be between 1 and 20.");

                task.RuleFor(t => t.TaskId)
                    .GreaterThan(0)
                    .WithMessage("TaskId must be greater than 0.");
            });
    }

    private static bool HaveUniquePriorities(List<ReorderTaskDto> list)
    {
        return list.Select(x => x.Priority).Distinct().Count() == list.Count;
    }
}