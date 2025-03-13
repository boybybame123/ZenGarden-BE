using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class SuggestFocusMethodValidator : AbstractValidator<SuggestFocusMethodDto>
{
    public SuggestFocusMethodValidator()
    {
        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("TaskName is required."); 
        
        RuleFor(x => x.TaskDescription)
            .NotEmpty().WithMessage("TaskDescription is required.");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate).WithMessage("StartDate must be before EndDate.");
    }
}
