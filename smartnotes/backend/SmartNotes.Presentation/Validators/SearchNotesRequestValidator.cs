using FluentValidation;
using SmartNotes.Application.DTOs;

namespace SmartNotes.Presentation.Validators;

public class SearchNotesRequestValidator : AbstractValidator<SearchNotesRequest>
{
    public SearchNotesRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .Length(1, 100).WithMessage("Keyword must be between 1 and 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Keyword));

        RuleFor(x => x.Tags)
            .Must(BeValidTags).WithMessage("Tags cannot be empty or contain only whitespace.")
            .Must(HaveValidTagCount).WithMessage("Maximum of 10 tags allowed for search.")
            .Must(HaveValidTagLength).WithMessage("Each tag must be between 1 and 50 characters.")
            .When(x => x.Tags != null);

        RuleFor(x => x)
            .Must(HaveAtLeastOneSearchCriteria)
            .WithMessage("At least one search criteria (keyword or tags) must be provided.");
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

    private static bool HaveAtLeastOneSearchCriteria(SearchNotesRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Keyword) || 
               (request.Tags != null && request.Tags.Any());
    }
}