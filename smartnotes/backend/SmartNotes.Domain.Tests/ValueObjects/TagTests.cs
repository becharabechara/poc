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
        Assert.Throws<ArgumentException>(() => (Tag)((string)null));
    }

    [Fact]
    public void ExplicitConversion_FromWhitespaceString_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => (Tag)"   ");
    }
}