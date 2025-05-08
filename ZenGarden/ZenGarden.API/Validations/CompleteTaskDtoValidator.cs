using FluentValidation;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Validations;

public class CompleteTaskDtoValidator : AbstractValidator<CompleteTaskDto>
{
    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB

    private static readonly string[] AllowedFileExtensions =
        { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".txt", ".zip" };

    public CompleteTaskDtoValidator()
    {
        // UserTreeId validation
        When(dto => dto.UserTreeId.HasValue, () =>
        {
            RuleFor(dto => dto.UserTreeId)
                .GreaterThan(0).WithMessage("UserTreeId must be a positive integer.");
        });

        // TaskNote validation
        RuleFor(dto => dto.TaskNote)
            .MaximumLength(2000).WithMessage("Task note cannot exceed 2000 characters.")
            .When(dto => !string.IsNullOrEmpty(dto.TaskNote));

        // TaskResult validation (URL format validation)
        When(dto => !string.IsNullOrEmpty(dto.TaskResult), () =>
        {
            RuleFor(dto => dto.TaskResult)
                .Must(BeAValidUrl).WithMessage("TaskResult must be a valid URL.");
        });

        // TaskFile validation
        When(dto => dto.TaskFile != null, () =>
        {
            RuleFor(dto => dto.TaskFile)
                .Must(BeAValidFile)
                .WithMessage(
                    $"Invalid file. Allowed extensions: {string.Join(", ", AllowedFileExtensions)}. Maximum size: 10MB.");
        });
    }

    private static bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private static bool BeAValidFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > MaxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return Array.Exists(AllowedFileExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }
}