using FluentAssertions;
using WebCrawler.Services;
using WebCrawler.Models;

namespace WebCrawler.Tests.Integration;

/// <summary>
/// Integration tests using exact examples from the exercise requirements.
/// Tests the complete web crawler workflow with file-based HTML content.
/// </summary>
public sealed class WebCrawlerIntegrationTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly WebCrawlerService _service;

    public WebCrawlerIntegrationTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        var fileBrowser = new FileBrowser();
        var emailExtractor = new EmailExtractor();
        _service = new WebCrawlerService(fileBrowser, emailExtractor);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public async Task CrawlAsync_WithExerciseExample_ReturnsCorrectResults()
    {
        // Arrange - Create the exact HTML files from the exercise
        await CreateExerciseFiles();
        var startUrl = Path.Combine(_tempDirectory, "index.html");

        // Act
        var result = await _service.CrawlAsync(startUrl, maxDepth: 2);

        // Assert
        result.Should().NotBeNull();
        result.StartUrl.Should().Be(startUrl);
        result.MaxDepth.Should().Be(2);
        result.TotalPages.Should().Be(3); // index.html, child1.html, child2.html

        // Verify all expected emails are found
        result.AllEmails.Should().BeEquivalentTo(new[]
        {
            "contact@example.com",
            "info@child1.com", 
            "support@child2.org"
        });

        // Verify page structure
        var indexPage = result.Pages.First(p => p.Url.EndsWith("index.html"));
        indexPage.Depth.Should().Be(0);
        indexPage.Emails.Should().Contain("contact@example.com");
        indexPage.Error.Should().BeNull();

        var child1Page = result.Pages.First(p => p.Url.EndsWith("child1.html"));
        child1Page.Depth.Should().Be(1);
        child1Page.Emails.Should().Contain("info@child1.com");
        child1Page.Error.Should().BeNull();

        var child2Page = result.Pages.First(p => p.Url.EndsWith("child2.html"));
        child2Page.Depth.Should().Be(1);
        child2Page.Emails.Should().Contain("support@child2.org");
        child2Page.Error.Should().BeNull();
    }

    [Fact]
    public async Task CrawlAsync_WithDepthLimit_RespectsMaxDepth()
    {
        // Arrange
        await CreateDeepHierarchyFiles();
        var startUrl = Path.Combine(_tempDirectory, "level0.html");

        // Act - Limit to depth 1
        var result = await _service.CrawlAsync(startUrl, maxDepth: 1);

        // Assert
        result.TotalPages.Should().Be(2); // level0.html and level1.html only
        result.Pages.Should().NotContain(p => p.Url.EndsWith("level2.html"));
        
        var level0 = result.Pages.First(p => p.Url.EndsWith("level0.html"));
        level0.Depth.Should().Be(0);
        
        var level1 = result.Pages.First(p => p.Url.EndsWith("level1.html"));
        level1.Depth.Should().Be(1);
    }

    [Fact]
    public async Task CrawlAsync_WithCircularReferences_HandlesCorrectly()
    {
        // Arrange
        await CreateCircularReferenceFiles();
        var startUrl = Path.Combine(_tempDirectory, "pageA.html");

        // Act
        var result = await _service.CrawlAsync(startUrl, maxDepth: 3);

        // Assert
        result.TotalPages.Should().Be(2); // Only pageA.html and pageB.html
        result.AllEmails.Should().BeEquivalentTo(new[] { "a@example.com", "b@example.com" });
        
        // Verify each page appears only once despite circular references
        result.Pages.Count(p => p.Url.EndsWith("pageA.html")).Should().Be(1);
        result.Pages.Count(p => p.Url.EndsWith("pageB.html")).Should().Be(1);
    }

    [Fact]
    public async Task CrawlAsync_WithMissingFiles_ContinuesProcessing()
    {
        // Arrange
        await CreateHtmlFile("index.html", @"
            <html>
                <body>
                    <h1>Main Page</h1>
                    <a href=""mailto:main@example.com"">Contact</a>
                    <a href=""existing.html"">Existing Page</a>
                    <a href=""missing.html"">Missing Page</a>
                </body>
            </html>");

        await CreateHtmlFile("existing.html", @"
            <html>
                <body>
                    <h1>Existing Page</h1>
                    <a href=""mailto:existing@example.com"">Email</a>
                </body>
            </html>");

        var startUrl = Path.Combine(_tempDirectory, "index.html");

        // Act
        var result = await _service.CrawlAsync(startUrl, maxDepth: 1);

        // Assert
        result.TotalPages.Should().Be(3); // index, existing, and missing (with error)
        result.AllEmails.Should().BeEquivalentTo(new[] { "main@example.com", "existing@example.com" });

        var missingPage = result.Pages.First(p => p.Url.EndsWith("missing.html"));
        missingPage.Error.Should().NotBeNullOrEmpty();
        missingPage.Emails.Should().BeEmpty();
    }

    [Fact]
    public async Task CrawlAsync_WithComplexEmailFormats_ExtractsAllEmails()
    {
        // Arrange
        await CreateHtmlFile("complex.html", @"
            <html>
                <body>
                    <h1>Complex Email Examples</h1>
                    <a href=""mailto:simple@example.com"">Simple</a>
                    <a href=""mailto:user.name@sub.domain.co.uk"">Complex domain</a>
                    <a href=""mailto:user+tag@example.org"">Plus sign</a>
                    <a href=""contact.php?email=form@example.com"">In query string</a>
                    <a href=""mailto:123numbers@example123.com"">With numbers</a>
                </body>
            </html>");

        var startUrl = Path.Combine(_tempDirectory, "complex.html");

        // Act
        var result = await _service.CrawlAsync(startUrl, maxDepth: 0);

        // Assert
        result.AllEmails.Should().BeEquivalentTo(new[]
        {
            "simple@example.com",
            "user.name@sub.domain.co.uk",
            "user+tag@example.org",
            "form@example.com",
            "123numbers@example123.com"
        });
    }

    [Fact]
    public async Task CrawlAsync_WithLargeWebsite_PerformsEfficiently()
    {
        // Arrange - Create a larger site structure
        await CreateLargeWebsiteFiles();
        var startUrl = Path.Combine(_tempDirectory, "hub.html");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.CrawlAsync(startUrl, maxDepth: 2);
        stopwatch.Stop();

        // Assert
        result.TotalPages.Should().BeGreaterThan(10);
        result.AllEmails.Should().HaveCountGreaterThan(10);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Should complete within 2 seconds
    }

    private async Task CreateExerciseFiles()
    {
        // index.html - Main page linking to two child pages
        await CreateHtmlFile("index.html", @"
            <html>
                <head><title>Main Page</title></head>
                <body>
                    <h1>Welcome to the Main Page</h1>
                    <p>Contact us at: <a href=""mailto:contact@example.com"">contact@example.com</a></p>
                    <nav>
                        <a href=""child1.html"">Go to Child 1</a>
                        <a href=""child2.html"">Go to Child 2</a>
                    </nav>
                </body>
            </html>");

        // child1.html - First child page
        await CreateHtmlFile("child1.html", @"
            <html>
                <head><title>Child Page 1</title></head>
                <body>
                    <h1>Child Page 1</h1>
                    <p>For more information: <a href=""mailto:info@child1.com"">info@child1.com</a></p>
                    <a href=""index.html"">Back to Main</a>
                </body>
            </html>");

        // child2.html - Second child page
        await CreateHtmlFile("child2.html", @"
            <html>
                <head><title>Child Page 2</title></head>
                <body>
                    <h1>Child Page 2</h1>
                    <p>Need support? <a href=""mailto:support@child2.org"">support@child2.org</a></p>
                    <a href=""index.html"">Back to Main</a>
                </body>
            </html>");
    }

    private async Task CreateDeepHierarchyFiles()
    {
        await CreateHtmlFile("level0.html", @"
            <html><body>
                <h1>Level 0</h1>
                <a href=""mailto:level0@example.com"">Email</a>
                <a href=""level1.html"">Next Level</a>
            </body></html>");

        await CreateHtmlFile("level1.html", @"
            <html><body>
                <h1>Level 1</h1>
                <a href=""mailto:level1@example.com"">Email</a>
                <a href=""level2.html"">Next Level</a>
            </body></html>");

        await CreateHtmlFile("level2.html", @"
            <html><body>
                <h1>Level 2</h1>
                <a href=""mailto:level2@example.com"">Email</a>
            </body></html>");
    }

    private async Task CreateCircularReferenceFiles()
    {
        await CreateHtmlFile("pageA.html", @"
            <html><body>
                <h1>Page A</h1>
                <a href=""mailto:a@example.com"">Email A</a>
                <a href=""pageB.html"">Go to B</a>
            </body></html>");

        await CreateHtmlFile("pageB.html", @"
            <html><body>
                <h1>Page B</h1>
                <a href=""mailto:b@example.com"">Email B</a>
                <a href=""pageA.html"">Go to A</a>
            </body></html>");
    }

    private async Task CreateLargeWebsiteFiles()
    {
        // Create a hub page linking to many sub-pages
        var hubLinks = string.Join("\n", 
            Enumerable.Range(1, 20).Select(i => $"<a href=\"page{i}.html\">Page {i}</a>"));
        
        await CreateHtmlFile("hub.html", $@"
            <html><body>
                <h1>Hub Page</h1>
                <a href=""mailto:hub@example.com"">Hub Email</a>
                <div>{hubLinks}</div>
            </body></html>");

        // Create individual pages
        for (int i = 1; i <= 20; i++)
        {
            await CreateHtmlFile($"page{i}.html", $@"
                <html><body>
                    <h1>Page {i}</h1>
                    <a href=""mailto:page{i}@example.com"">Page {i} Email</a>
                    <a href=""hub.html"">Back to Hub</a>
                </body></html>");
        }
    }

    private async Task CreateHtmlFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);
    }
}