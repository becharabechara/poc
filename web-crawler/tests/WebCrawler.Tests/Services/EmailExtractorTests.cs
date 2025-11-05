using FluentAssertions;
using WebCrawler.Services;

namespace WebCrawler.Tests.Services;

public sealed class EmailExtractorTests
{
    private readonly EmailExtractor _extractor;

    public EmailExtractorTests()
    {
        _extractor = new EmailExtractor();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ExtractEmails_WithNullOrEmptyContent_ReturnsEmptyCollection(string? content)
    {
        // Act
        var result = _extractor.ExtractEmails(content!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractEmails_WithNoEmails_ReturnsEmptyCollection()
    {
        // Arrange
        const string content = "<html><body><p>No emails here!</p></body></html>";

        // Act
        var result = _extractor.ExtractEmails(content);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractEmails_WithMailtoLinks_ExtractsEmails()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""mailto:test@example.com"">Contact</a>
                    <a href=""mailto:support@company.org"">Support</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractEmails(content);

        // Assert
        result.Should().BeEquivalentTo(new[] { "test@example.com", "support@company.org" });
    }

    [Fact]
    public void ExtractEmails_WithHrefEmails_ExtractsEmails()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""mailto:direct@example.com"">Direct</a>
                    <a href=""https://example.com/contact?email=indirect@example.com"">Indirect</a>
                    <p>Contact us at: <a href=""mailto:contact@domain.co.uk"">contact@domain.co.uk</a></p>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractEmails(content);

        // Assert
        result.Should().BeEquivalentTo(new[] 
        { 
            "direct@example.com", 
            "indirect@example.com", 
            "contact@domain.co.uk" 
        });
    }

    [Fact]
    public void ExtractEmails_WithDuplicateEmails_ReturnsUniqueEmails()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""mailto:test@example.com"">First</a>
                    <a href=""mailto:TEST@EXAMPLE.COM"">Second (case different)</a>
                    <a href=""mailto:test@example.com"">Third (exact duplicate)</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractEmails(content);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain("test@example.com");
    }

    [Fact]
    public void ExtractEmails_WithComplexEmailFormats_ExtractsCorrectly()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""mailto:user.name@example.com"">Dot in username</a>
                    <a href=""mailto:user+tag@example.co.uk"">Plus sign</a>
                    <a href=""mailto:user_name@sub.example.org"">Underscore and subdomain</a>
                    <a href=""mailto:123numbers@example123.com"">Numbers</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractEmails(content);

        // Assert
        result.Should().BeEquivalentTo(new[] 
        { 
            "user.name@example.com",
            "user+tag@example.co.uk",
            "user_name@sub.example.org",
            "123numbers@example123.com"
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ExtractLinks_WithNullOrEmptyContent_ReturnsEmptyCollection(string? content)
    {
        // Act
        var result = _extractor.ExtractLinks(content!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractLinks_WithNoLinks_ReturnsEmptyCollection()
    {
        // Arrange
        const string content = "<html><body><p>No links here!</p></body></html>";

        // Act
        var result = _extractor.ExtractLinks(content);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractLinks_WithHtmlLinks_ExtractsLinks()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""page1.html"">Page 1</a>
                    <a href=""./page2.html"">Page 2</a>
                    <a href=""../parent.html"">Parent</a>
                    <a href=""/absolute/path.html"">Absolute</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractLinks(content);

        // Assert
        result.Should().BeEquivalentTo(new[] 
        { 
            "page1.html", 
            "./page2.html", 
            "../parent.html",
            "/absolute/path.html"
        });
    }

    [Fact]
    public void ExtractLinks_WithDuplicateLinks_ReturnsUniqueLinks()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""page.html"">First</a>
                    <a href=""page.html"">Second (duplicate)</a>
                    <a href=""PAGE.HTML"">Third (case different)</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractLinks(content);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain("page.html");
    }

    [Fact]
    public void ExtractLinks_WithMailtoAndHttpLinks_ExtractsOnlyRelativeLinks()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""page.html"">Local page</a>
                    <a href=""mailto:test@example.com"">Email</a>
                    <a href=""https://external.com"">External</a>
                    <a href=""http://another.com"">Another external</a>
                    <a href=""./local.html"">Another local</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractLinks(content);

        // Assert
        result.Should().BeEquivalentTo(new[] { "page.html", "./local.html" });
    }

    [Fact]
    public void ExtractLinks_WithMalformedHtml_ExtractsValidLinks()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""good.html"">Good link</a>
                    <a href="""">Empty href</a>
                    <a>No href attribute</a>
                    <a href=""   "">Whitespace only</a>
                    <a href=""another.html"">Another good</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractLinks(content);

        // Assert
        result.Should().BeEquivalentTo(new[] { "good.html", "another.html" });
    }

    [Fact]
    public void ExtractEmails_WithMalformedHtml_ExtractsValidEmails()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""mailto:good@example.com"">Good email</a>
                    <a href=""mailto:"">Empty email</a>
                    <a href=""mailto:invalid-email"">Invalid email</a>
                    <a href=""mailto:another@domain.com"">Another good</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractEmails(content);

        // Assert
        result.Should().BeEquivalentTo(new[] { "good@example.com", "another@domain.com" });
    }

    [Fact]
    public void ExtractEmails_WithEmailsInQueryStrings_ExtractsEmails()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""contact.html?email=query@example.com"">Contact form</a>
                    <a href=""mailto:direct@example.com"">Direct email</a>
                    <a href=""form.php?user=test@domain.org&action=send"">Form with email</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractEmails(content);

        // Assert
        result.Should().BeEquivalentTo(new[] 
        { 
            "query@example.com", 
            "direct@example.com", 
            "test@domain.org" 
        });
    }

    [Fact]
    public void ExtractEmails_WithCaseInsensitiveMatching_ReturnsLowercaseEmails()
    {
        // Arrange
        const string content = @"
            <html>
                <body>
                    <a href=""mailto:Test@EXAMPLE.COM"">Mixed case</a>
                    <a href=""mailto:UPPER@DOMAIN.ORG"">Upper case</a>
                </body>
            </html>";

        // Act
        var result = _extractor.ExtractEmails(content);

        // Assert
        result.Should().BeEquivalentTo(new[] { "Test@EXAMPLE.COM", "UPPER@DOMAIN.ORG" });
        // Note: We preserve original case but deduplicate case-insensitively
    }

    [Fact]
    public void ExtractLinks_PerformanceTest_HandlesLargeContent()
    {
        // Arrange
        var links = Enumerable.Range(1, 1000).Select(i => $"<a href=\"page{i}.html\">Page {i}</a>");
        var content = $"<html><body>{string.Join("\n", links)}</body></html>";

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = _extractor.ExtractLinks(content);
        stopwatch.Stop();

        // Assert
        result.Should().HaveCount(1000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should be very fast with compiled regex
    }
}