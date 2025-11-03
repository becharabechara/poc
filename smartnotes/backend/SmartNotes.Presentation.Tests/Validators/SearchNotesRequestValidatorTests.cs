using FluentValidation.TestHelper;
using SmartNotes.Application.DTOs;
using SmartNotes.Presentation.Validators;

namespace SmartNotes.Presentation.Tests.Validators;

public class SearchNotesRequestValidatorTests
{
    private readonly SearchNotesRequestValidator _validator;

    public SearchNotesRequestValidatorTests()
    {
        _validator = new SearchNotesRequestValidator();
    }

    [Fact]
    public void Validate_WhenKeywordIsValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Keyword = "valid keyword" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Keyword);
    }

    [Fact]
    public void Validate_WhenKeywordIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Keyword = new string('a', 101) };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Keyword)
            .WithErrorMessage("Keyword must be between 1 and 100 characters.");
    }

    [Fact]
    public void Validate_WhenKeywordIsEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Keyword = "" };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Keyword);
    }

    [Fact]
    public void Validate_WhenKeywordIsNull_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Keyword = null };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Keyword);
    }

    [Fact]
    public void Validate_WhenTagsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Tags = new[] { "tag1", "tag2" } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_WhenTagsContainEmptyString_ShouldHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Tags = new[] { "valid", "" } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Tags cannot be empty or contain only whitespace.");
    }

    [Fact]
    public void Validate_WhenTagsContainWhitespaceOnly_ShouldHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Tags = new[] { "valid", "   " } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Tags cannot be empty or contain only whitespace.");
    }

    [Fact]
    public void Validate_WhenTooManyTags_ShouldHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Tags = Enumerable.Range(1, 11).Select(i => $"tag{i}").ToArray() };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Maximum of 10 tags allowed for search.");
    }

    [Fact]
    public void Validate_WhenTagIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Tags = new[] { new string('a', 51) } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Each tag must be between 1 and 50 characters.");
    }

    [Fact]
    public void Validate_WhenTagIsTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Tags = new[] { "" } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Each tag must be between 1 and 50 characters.");
    }

    [Fact]
    public void Validate_WhenNoSearchCriteriaProvided_ShouldHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Keyword = null, Tags = null };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("At least one search criteria (keyword or tags) must be provided.");
    }

    [Fact]
    public void Validate_WhenKeywordProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Keyword = "test", Tags = null };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenTagsProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Keyword = null, Tags = new[] { "tag" } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenBothKeywordAndTagsProvided_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new SearchNotesRequest { Keyword = "test", Tags = new[] { "tag" } };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x);
    }
}