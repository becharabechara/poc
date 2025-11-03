using FluentValidation.TestHelper;
using SmartNotes.Application.DTOs;
using SmartNotes.Presentation.Validators;

namespace SmartNotes.Presentation.Tests.Validators;

public class CreateNoteRequestValidatorTests
{
    private readonly CreateNoteRequestValidator _validator;

    public CreateNoteRequestValidatorTests()
    {
        _validator = new CreateNoteRequestValidator();
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = "Valid content for the note",
            Tags = new[] { "tag1", "tag2" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = null!,
            Content = "Valid content",
            Tags = new[] { "tag1" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_WhenTitleIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "",
            Content = "Valid content",
            Tags = new[] { "tag1" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_WhenTitleIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = new string('a', 201),
            Content = "Valid content",
            Tags = new[] { "tag1" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must be between 1 and 200 characters.");
    }

    [Fact]
    public void Validate_WhenContentIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = null!,
            Tags = new[] { "tag1" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content is required.");
    }

    [Fact]
    public void Validate_WhenContentIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = "",
            Tags = new[] { "tag1" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content is required.");
    }

    [Fact]
    public void Validate_WhenContentIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = new string('a', 10001),
            Tags = new[] { "tag1" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content must be between 1 and 10,000 characters.");
    }

    [Fact]
    public void Validate_WhenTagsAreNull_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = "Valid content",
            Tags = null
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_WhenTagsAreEmpty_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = "Valid content",
            Tags = Array.Empty<string>()
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void Validate_WhenTooManyTags_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = "Valid content",
            Tags = Enumerable.Range(1, 11).Select(i => $"tag{i}").ToArray()
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Maximum of 10 tags allowed.");
    }

    [Fact]
    public void Validate_WhenTagIsTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = "Valid content",
            Tags = new[] { new string('a', 51) }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Each tag must be between 1 and 50 characters.");
    }

    [Fact]
    public void Validate_WhenTagIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = "Valid content",
            Tags = new[] { "" }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Tags cannot be empty or contain only whitespace.");
    }

    [Fact]
    public void Validate_WhenTagContainsOnlyWhitespace_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Valid Title",
            Content = "Valid content",
            Tags = new[] { "   " }
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Tags)
            .WithErrorMessage("Tags cannot be empty or contain only whitespace.");
    }
}