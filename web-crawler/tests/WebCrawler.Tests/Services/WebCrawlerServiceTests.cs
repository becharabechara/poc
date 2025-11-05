using FluentAssertions;
using Moq;
using WebCrawler.Interfaces;
using WebCrawler.Services;
using WebCrawler.Models;

namespace WebCrawler.Tests.Services;

public sealed class WebCrawlerServiceTests
{
    private readonly Mock<IWebBrowser> _mockWebBrowser;
    private readonly Mock<IEmailExtractor> _mockEmailExtractor;
    private readonly WebCrawlerService _service;

    public WebCrawlerServiceTests()
    {
        _mockWebBrowser = new Mock<IWebBrowser>();
        _mockEmailExtractor = new Mock<IEmailExtractor>();
        _service = new WebCrawlerService(_mockWebBrowser.Object, _mockEmailExtractor.Object);
    }

    [Fact]
    public void Constructor_WithNullWebBrowser_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new WebCrawlerService(null!, _mockEmailExtractor.Object));
        
        exception.ParamName.Should().Be("webBrowser");
    }

    [Fact]
    public void Constructor_WithNullEmailExtractor_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new WebCrawlerService(_mockWebBrowser.Object, null!));
        
        exception.ParamName.Should().Be("emailExtractor");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CrawlAsync_WithInvalidStartUrl_ThrowsArgumentException(string? startUrl)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CrawlAsync(startUrl!, 1));
        
        exception.ParamName.Should().Be("startUrl");
    }

    [Fact]
    public async Task CrawlAsync_WithNegativeMaxDepth_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CrawlAsync("index.html", -1));
        
        exception.ParamName.Should().Be("maxDepth");
    }

    [Fact]
    public async Task CrawlAsync_WithSinglePage_ReturnsCorrectResult()
    {
        // Arrange
        const string startUrl = "index.html";
        const string content = "<html><body><a href=\"mailto:test@example.com\">Email</a></body></html>";
        var emails = new HashSet<string> { "test@example.com" };
        var links = new HashSet<string>();

        _mockWebBrowser.Setup(x => x.GetContentAsync(startUrl))
            .ReturnsAsync(content);
        _mockEmailExtractor.Setup(x => x.ExtractEmails(content))
            .Returns(emails);
        _mockEmailExtractor.Setup(x => x.ExtractLinks(content))
            .Returns(links);

        // Act
        var result = await _service.CrawlAsync(startUrl, 0);

        // Assert
        result.Should().NotBeNull();
        result.StartUrl.Should().Be(startUrl);
        result.MaxDepth.Should().Be(0);
        result.TotalPages.Should().Be(1);
        result.AllEmails.Should().BeEquivalentTo(emails);
        result.Pages.Should().HaveCount(1);
        
        var page = result.Pages.First();
        page.Url.Should().Be(startUrl);
        page.Depth.Should().Be(0);
        page.Emails.Should().BeEquivalentTo(emails);
        page.Links.Should().BeEmpty();
        page.Error.Should().BeNull();
    }

    [Fact]
    public async Task CrawlAsync_WithMultiplePages_PerformsBreadthFirstSearch()
    {
        // Arrange
        const string startUrl = "index.html";
        const string childUrl = "child.html";
        
        var indexContent = "<html><body><a href=\"child.html\">Child</a><a href=\"mailto:index@example.com\">Email</a></body></html>";
        var childContent = "<html><body><a href=\"mailto:child@example.com\">Email</a></body></html>";
        
        var indexEmails = new HashSet<string> { "index@example.com" };
        var childEmails = new HashSet<string> { "child@example.com" };
        var indexLinks = new HashSet<string> { "child.html" };
        var childLinks = new HashSet<string>();

        _mockWebBrowser.Setup(x => x.GetContentAsync(startUrl))
            .ReturnsAsync(indexContent);
        _mockWebBrowser.Setup(x => x.GetContentAsync(childUrl))
            .ReturnsAsync(childContent);
        _mockWebBrowser.Setup(x => x.ResolveUrl(startUrl, "child.html"))
            .Returns(childUrl);
        
        _mockEmailExtractor.Setup(x => x.ExtractEmails(indexContent))
            .Returns(indexEmails);
        _mockEmailExtractor.Setup(x => x.ExtractEmails(childContent))
            .Returns(childEmails);
        _mockEmailExtractor.Setup(x => x.ExtractLinks(indexContent))
            .Returns(indexLinks);
        _mockEmailExtractor.Setup(x => x.ExtractLinks(childContent))
            .Returns(childLinks);

        // Act
        var result = await _service.CrawlAsync(startUrl, 1);

        // Assert
        result.Should().NotBeNull();
        result.TotalPages.Should().Be(2);
        result.AllEmails.Should().BeEquivalentTo(new[] { "index@example.com", "child@example.com" });
        
        var indexPage = result.Pages.First(p => p.Url == startUrl);
        indexPage.Depth.Should().Be(0);
        indexPage.Emails.Should().BeEquivalentTo(indexEmails);
        
        var childPage = result.Pages.First(p => p.Url == childUrl);
        childPage.Depth.Should().Be(1);
        childPage.Emails.Should().BeEquivalentTo(childEmails);
    }

    [Fact]
    public async Task CrawlAsync_WithMaxDepthReached_StopsAtMaxDepth()
    {
        // Arrange
        const string startUrl = "index.html";
        const string childUrl = "child.html";
        const string grandchildUrl = "grandchild.html";
        
        var indexContent = "<html><body><a href=\"child.html\">Child</a></body></html>";
        var childContent = "<html><body><a href=\"grandchild.html\">Grandchild</a></body></html>";
        
        var indexLinks = new HashSet<string> { "child.html" };
        var childLinks = new HashSet<string> { "grandchild.html" };

        _mockWebBrowser.Setup(x => x.GetContentAsync(startUrl))
            .ReturnsAsync(indexContent);
        _mockWebBrowser.Setup(x => x.GetContentAsync(childUrl))
            .ReturnsAsync(childContent);
        _mockWebBrowser.Setup(x => x.ResolveUrl(startUrl, "child.html"))
            .Returns(childUrl);
        _mockWebBrowser.Setup(x => x.ResolveUrl(childUrl, "grandchild.html"))
            .Returns(grandchildUrl);
        
        _mockEmailExtractor.Setup(x => x.ExtractEmails(It.IsAny<string>()))
            .Returns(new HashSet<string>());
        _mockEmailExtractor.Setup(x => x.ExtractLinks(indexContent))
            .Returns(indexLinks);
        _mockEmailExtractor.Setup(x => x.ExtractLinks(childContent))
            .Returns(childLinks);

        // Act
        var result = await _service.CrawlAsync(startUrl, 1);

        // Assert
        result.TotalPages.Should().Be(2);
        result.Pages.Should().NotContain(p => p.Url == grandchildUrl);
        
        // Verify grandchild was not requested
        _mockWebBrowser.Verify(x => x.GetContentAsync(grandchildUrl), Times.Never);
    }

    [Fact]
    public async Task CrawlAsync_WithCircularReferences_PreventsInfiniteLoop()
    {
        // Arrange
        const string startUrl = "page1.html";
        const string page2Url = "page2.html";
        
        var page1Content = "<html><body><a href=\"page2.html\">Page 2</a></body></html>";
        var page2Content = "<html><body><a href=\"page1.html\">Page 1</a></body></html>";
        
        var page1Links = new HashSet<string> { "page2.html" };
        var page2Links = new HashSet<string> { "page1.html" };

        _mockWebBrowser.Setup(x => x.GetContentAsync(startUrl))
            .ReturnsAsync(page1Content);
        _mockWebBrowser.Setup(x => x.GetContentAsync(page2Url))
            .ReturnsAsync(page2Content);
        _mockWebBrowser.Setup(x => x.ResolveUrl(startUrl, "page2.html"))
            .Returns(page2Url);
        _mockWebBrowser.Setup(x => x.ResolveUrl(page2Url, "page1.html"))
            .Returns(startUrl);
        
        _mockEmailExtractor.Setup(x => x.ExtractEmails(It.IsAny<string>()))
            .Returns(new HashSet<string>());
        _mockEmailExtractor.Setup(x => x.ExtractLinks(page1Content))
            .Returns(page1Links);
        _mockEmailExtractor.Setup(x => x.ExtractLinks(page2Content))
            .Returns(page2Links);

        // Act
        var result = await _service.CrawlAsync(startUrl, 2);

        // Assert
        result.TotalPages.Should().Be(2);
        
        // Verify each page was only requested once
        _mockWebBrowser.Verify(x => x.GetContentAsync(startUrl), Times.Once);
        _mockWebBrowser.Verify(x => x.GetContentAsync(page2Url), Times.Once);
    }

    [Fact]
    public async Task CrawlAsync_WithPageError_ContinuesWithOtherPages()
    {
        // Arrange
        const string startUrl = "index.html";
        const string workingUrl = "working.html";
        const string errorUrl = "error.html";
        
        var indexContent = "<html><body><a href=\"working.html\">Working</a><a href=\"error.html\">Error</a></body></html>";
        var workingContent = "<html><body><a href=\"mailto:working@example.com\">Email</a></body></html>";
        
        var indexLinks = new HashSet<string> { "working.html", "error.html" };
        var workingEmails = new HashSet<string> { "working@example.com" };

        _mockWebBrowser.Setup(x => x.GetContentAsync(startUrl))
            .ReturnsAsync(indexContent);
        _mockWebBrowser.Setup(x => x.GetContentAsync(workingUrl))
            .ReturnsAsync(workingContent);
        _mockWebBrowser.Setup(x => x.GetContentAsync(errorUrl))
            .ThrowsAsync(new InvalidOperationException("File not found"));
        _mockWebBrowser.Setup(x => x.ResolveUrl(startUrl, "working.html"))
            .Returns(workingUrl);
        _mockWebBrowser.Setup(x => x.ResolveUrl(startUrl, "error.html"))
            .Returns(errorUrl);
        
        _mockEmailExtractor.Setup(x => x.ExtractEmails(indexContent))
            .Returns(new HashSet<string>());
        _mockEmailExtractor.Setup(x => x.ExtractEmails(workingContent))
            .Returns(workingEmails);
        _mockEmailExtractor.Setup(x => x.ExtractLinks(indexContent))
            .Returns(indexLinks);
        _mockEmailExtractor.Setup(x => x.ExtractLinks(workingContent))
            .Returns(new HashSet<string>());

        // Act
        var result = await _service.CrawlAsync(startUrl, 1);

        // Assert
        result.TotalPages.Should().Be(3);
        result.AllEmails.Should().BeEquivalentTo(workingEmails);
        
        var errorPage = result.Pages.First(p => p.Url == errorUrl);
        errorPage.Error.Should().NotBeNullOrEmpty();
        errorPage.Emails.Should().BeEmpty();
        
        var workingPage = result.Pages.First(p => p.Url == workingUrl);
        workingPage.Error.Should().BeNull();
        workingPage.Emails.Should().BeEquivalentTo(workingEmails);
    }

    [Fact]
    public async Task CrawlAsync_WithZeroDepth_ProcessesOnlyStartPage()
    {
        // Arrange
        const string startUrl = "index.html";
        const string content = "<html><body><a href=\"child.html\">Child</a><a href=\"mailto:test@example.com\">Email</a></body></html>";
        var emails = new HashSet<string> { "test@example.com" };
        var links = new HashSet<string> { "child.html" };

        _mockWebBrowser.Setup(x => x.GetContentAsync(startUrl))
            .ReturnsAsync(content);
        _mockEmailExtractor.Setup(x => x.ExtractEmails(content))
            .Returns(emails);
        _mockEmailExtractor.Setup(x => x.ExtractLinks(content))
            .Returns(links);

        // Act
        var result = await _service.CrawlAsync(startUrl, 0);

        // Assert
        result.TotalPages.Should().Be(1);
        result.AllEmails.Should().BeEquivalentTo(emails);
        
        var page = result.Pages.First();
        page.Links.Should().BeEmpty(); // Links not extracted at max depth
        
        // Verify no attempt to resolve links at max depth
        _mockWebBrowser.Verify(x => x.ResolveUrl(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}