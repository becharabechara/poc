using FluentAssertions;
using WebCrawler.Services;

namespace WebCrawler.Tests.Services;

public sealed class FileBrowserTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileBrowser _fileBrowser;

    public FileBrowserTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _fileBrowser = new FileBrowser();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetContentAsync_WithInvalidUrl_ThrowsArgumentException(string? url)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _fileBrowser.GetContentAsync(url!));
        
        exception.ParamName.Should().Be("url");
    }

    [Fact]
    public async Task GetContentAsync_WithNonExistentFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_tempDirectory, "nonexistent.html");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _fileBrowser.GetContentAsync(nonExistentFile));
        
        exception.Message.Should().StartWith("File not found:");
    }

    [Fact]
    public async Task GetContentAsync_WithValidFile_ReturnsContent()
    {
        // Arrange
        const string expectedContent = "<html><body>Test content</body></html>";
        var testFile = Path.Combine(_tempDirectory, "test.html");
        await File.WriteAllTextAsync(testFile, expectedContent);

        // Act
        var result = await _fileBrowser.GetContentAsync(testFile);

        // Assert
        result.Should().Be(expectedContent);
    }

    [Fact]
    public async Task GetContentAsync_WithFileUriScheme_ReturnsContent()
    {
        // Arrange
        const string expectedContent = "<html><body>Test content</body></html>";
        var testFile = Path.Combine(_tempDirectory, "test.html");
        await File.WriteAllTextAsync(testFile, expectedContent);
        
        var fileUri = $"file://{testFile.Replace('\\', '/')}";

        // Act
        var result = await _fileBrowser.GetContentAsync(fileUri);

        // Assert
        result.Should().Be(expectedContent);
    }

    [Theory]
    [InlineData(null, "child.html")]
    [InlineData("", "child.html")]
    [InlineData("   ", "child.html")]
    [InlineData("base.html", null)]
    [InlineData("base.html", "")]
    [InlineData("base.html", "   ")]
    public void ResolveUrl_WithInvalidParameters_ThrowsArgumentException(string? baseUrl, string? relativeUrl)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            _fileBrowser.ResolveUrl(baseUrl!, relativeUrl!));
        
        exception.ParamName.Should().BeOneOf("baseUrl", "relativeUrl");
    }

    [Fact]
    public void ResolveUrl_WithRelativePathCurrentDirectory_ReturnsCorrectPath()
    {
        // Arrange
        var baseFile = Path.Combine(_tempDirectory, "index.html");
        const string relativeUrl = "./child.html";

        // Act
        var result = _fileBrowser.ResolveUrl(baseFile, relativeUrl);

        // Assert
        var expected = Path.Combine(_tempDirectory, "child.html");
        result.Should().Be(Path.GetFullPath(expected));
    }

    [Fact]
    public void ResolveUrl_WithRelativePathParentDirectory_ReturnsCorrectPath()
    {
        // Arrange
        var subDirectory = Path.Combine(_tempDirectory, "sub");
        Directory.CreateDirectory(subDirectory);
        var baseFile = Path.Combine(subDirectory, "index.html");
        const string relativeUrl = "../parent.html";

        // Act
        var result = _fileBrowser.ResolveUrl(baseFile, relativeUrl);

        // Assert
        var expected = Path.Combine(_tempDirectory, "parent.html");
        result.Should().Be(Path.GetFullPath(expected));
    }

    [Fact]
    public void ResolveUrl_WithRelativePathNoPrefix_ReturnsCorrectPath()
    {
        // Arrange
        var baseFile = Path.Combine(_tempDirectory, "index.html");
        const string relativeUrl = "child.html";

        // Act
        var result = _fileBrowser.ResolveUrl(baseFile, relativeUrl);

        // Assert
        var expected = Path.Combine(_tempDirectory, "child.html");
        result.Should().Be(Path.GetFullPath(expected));
    }

    [Fact]
    public void ResolveUrl_WithAbsolutePath_ReturnsNormalizedPath()
    {
        // Arrange
        var baseFile = Path.Combine(_tempDirectory, "index.html");
        var absoluteUrl = Path.Combine(_tempDirectory, "other.html");

        // Act
        var result = _fileBrowser.ResolveUrl(baseFile, absoluteUrl);

        // Assert
        result.Should().Be(Path.GetFullPath(absoluteUrl));
    }

    [Fact]
    public void ResolveUrl_WithForwardSlashes_NormalizesToSystemPaths()
    {
        // Arrange
        var baseFile = _tempDirectory.Replace('\\', '/') + "/index.html";
        const string relativeUrl = "./child.html";

        // Act
        var result = _fileBrowser.ResolveUrl(baseFile, relativeUrl);

        // Assert
        var expected = Path.Combine(_tempDirectory, "child.html");
        result.Should().Be(Path.GetFullPath(expected));
    }

    [Fact]
    public void ResolveUrl_WithFileUriScheme_HandlesCorrectly()
    {
        // Arrange
        var baseFile = $"file://{_tempDirectory.Replace('\\', '/')}/index.html";
        const string relativeUrl = "child.html";

        // Act
        var result = _fileBrowser.ResolveUrl(baseFile, relativeUrl);

        // Assert
        var expected = Path.Combine(_tempDirectory, "child.html");
        result.Should().Be(Path.GetFullPath(expected));
    }

    [Fact]
    public async Task GetContentAsync_WithLargeFile_ReturnsFullContent()
    {
        // Arrange
        var largeContent = string.Join("\n", Enumerable.Range(1, 1000).Select(i => $"<p>Line {i}</p>"));
        var testFile = Path.Combine(_tempDirectory, "large.html");
        await File.WriteAllTextAsync(testFile, largeContent);

        // Act
        var result = await _fileBrowser.GetContentAsync(testFile);

        // Assert
        result.Should().Be(largeContent);
        result.Length.Should().BeGreaterThan(10000);
    }

    [Fact]
    public async Task GetContentAsync_WithUtf8Content_ReturnsCorrectEncoding()
    {
        // Arrange
        const string utf8Content = "<html><body>Tëst wïth spëcïàl chäräctërs: αβγδε</body></html>";
        var testFile = Path.Combine(_tempDirectory, "utf8.html");
        await File.WriteAllTextAsync(testFile, utf8Content, System.Text.Encoding.UTF8);

        // Act
        var result = await _fileBrowser.GetContentAsync(testFile);

        // Assert
        result.Should().Be(utf8Content);
    }
}