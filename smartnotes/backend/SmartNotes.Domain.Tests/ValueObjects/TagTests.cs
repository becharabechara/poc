using System;
using SmartNotes.Domain.ValueObjects;
using Xunit;

namespace SmartNotes.Domain.Tests.ValueObjects;

public class TagTests
{
    [Fact]
    public void CreateTag_WithValidValue_ShouldSucceed()
    {
        var value = "TestTag";
        var tag = new Tag(value);

        Assert.Equal(value.ToLower(), tag.Value);
    }

    [Fact]
    public void CreateTag_WithEmptyValue_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => new Tag(""));
    }

    [Fact]
    public void CreateTag_WithWhitespaceValue_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => new Tag("   "));
    }

    [Fact]
    public void CreateTag_WithValueTooLong_ShouldThrowException()
    {
        var longValue = new string('a', 51);
        Assert.Throws<ArgumentException>(() => new Tag(longValue));
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        var tag = new Tag("test");
        string value = tag;

        Assert.Equal("test", value);
    }

    [Fact]
    public void ExplicitConversion_FromString_ShouldCreateTag()
    {
        Tag tag = (Tag)"test";

        Assert.Equal("test", tag.Value);
    }

    [Fact]
    public void CreateTag_WithValueContainingSpecialCharacters_ShouldSucceed()
    {
        var tag = new Tag("test-tag_123");
        Assert.Equal("test-tag_123", tag.Value);
    }

    [Fact]
    public void CreateTag_WithMixedCase_ShouldNormalizeToLowercase()
    {
        var tag = new Tag("TestTag");
        Assert.Equal("testtag", tag.Value);
    }

    [Fact]
    public void ExplicitConversion_FromNullString_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => (Tag)((string?)null!));
    }

    [Fact]
    public void ExplicitConversion_FromWhitespaceString_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => (Tag)"   ");
    }

    [Fact]
    public void Tag_ShouldImplementValueSemantics()
    {
        // Arrange
        var tag1 = new Tag("test");
        var tag2 = new Tag("test");
        var tag3 = new Tag("different");

        // Act & Assert - Value equality
        Assert.Equal(tag1, tag2);
        Assert.NotEqual(tag1, tag3);
        Assert.True(tag1 == tag2);
        Assert.False(tag1 == tag3);
        Assert.False(tag1 != tag2);
        Assert.True(tag1 != tag3);
    }

    [Fact]
    public void Tag_GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        var tag1 = new Tag("test");
        var tag2 = new Tag("test");

        // Act & Assert
        Assert.Equal(tag1.GetHashCode(), tag2.GetHashCode());
    }

    [Fact]
    public void Tag_ToString_ShouldReturnValue()
    {
        // Arrange
        var tag = new Tag("test");

        // Act & Assert
        // Note: ToString for records returns the full record representation
        Assert.Contains("test", tag.ToString());
    }

    [Fact]
    public void CreateTag_WithMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxLengthValue = new string('a', 50);

        // Act
        var tag = new Tag(maxLengthValue);

        // Assert
        Assert.Equal(maxLengthValue, tag.Value);
    }

    [Fact]
    public void CreateTag_WithValueExceedingMaxLength_ShouldThrowException()
    {
        // Arrange
        var tooLongValue = new string('a', 51);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Tag(tooLongValue));
        Assert.Contains("Tag cannot exceed 50 characters", exception.Message);
    }

    [Fact]
    public void CreateTag_WithNullValue_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Tag(null!));
        Assert.Contains("Tag cannot be empty", exception.Message);
    }

    [Fact]
    public void CreateTag_WithEmptyString_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Tag(string.Empty));
        Assert.Contains("Tag cannot be empty", exception.Message);
    }

    [Fact]
    public void CreateTag_WithUnicodeCharacters_ShouldSucceed()
    {
        // Arrange & Act
        var tag = new Tag("测试-tag");

        // Assert
        Assert.Equal("测试-tag", tag.Value);
    }

    [Fact]
    public void CreateTag_WithNumbers_ShouldSucceed()
    {
        // Arrange & Act
        var tag = new Tag("tag123");

        // Assert
        Assert.Equal("tag123", tag.Value);
    }
}