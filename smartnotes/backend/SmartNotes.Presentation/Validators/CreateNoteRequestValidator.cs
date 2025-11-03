using FluentValidation;
using SmartNotes.Application.DTOs;

namespace SmartNotes.Presentation.Validators;

public class CreateNoteRequestValidator : AbstractValidator<CreateNoteRequest>
{
    public CreateNoteRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(1, 200).WithMessage("Title must be between 1 and 200 characters.")
            .Must(NotBeOnlyWhitespace).WithMessage("Title cannot contain only whitespace.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .Length(1, 10000).WithMessage("Content must be between 1 and 10,000 characters.")
            .Must(NotBeOnlyWhitespace).WithMessage("Content cannot contain only whitespace.");

        RuleFor(x => x.Tags)
            .Must(BeValidTags).WithMessage("Tags cannot be empty or contain only whitespace.")
            .Must(HaveValidTagCount).WithMessage("Maximum of 10 tags allowed.")
            .Must(HaveValidTagLength).WithMessage("Each tag must be between 1 and 50 characters.");
    }

    private static bool NotBeOnlyWhitespace(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool BeValidTags(IEnumerable<string>? tags)
    {
        if (tags == null) return true;
        
        return tags.All(tag => !string.IsNullOrWhiteSpace(tag));
    }

    private static bool HaveValidTagCount(IEnumerable<string>? tags)
    {
        if (tags == null) return true;
        
        return tags.Count() <= 10;
    }

    private static bool HaveValidTagLength(IEnumerable<string>? tags)
    {
        if (tags == null) return true;
        
        return tags.All(tag => tag?.Length >= 1 && tag?.Length <= 50);
    }
}