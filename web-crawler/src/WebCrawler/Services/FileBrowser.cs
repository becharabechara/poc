using WebCrawler.Interfaces;

namespace WebCrawler.Services;

public sealed class FileBrowser : IWebBrowser
{
    public async Task<string> GetContentAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        try
        {
            var filePath = NormalizeFilePath(url);
            
            if (!File.Exists(filePath))
                throw new InvalidOperationException($"File not found: {filePath}");

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Failed to read file: {url}", ex);
        }
    }

    public string ResolveUrl(string baseUrl, string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        
        if (string.IsNullOrWhiteSpace(relativeUrl))
            throw new ArgumentException("Relative URL cannot be null or empty", nameof(relativeUrl));

        try
        {
            var baseFilePath = NormalizeFilePath(baseUrl);
            var baseDirectory = Path.GetDirectoryName(baseFilePath) ?? string.Empty;

            if (relativeUrl.StartsWith("./"))
            {
                var fileName = relativeUrl.Substring(2);
                return Path.Combine(baseDirectory, fileName);
            }
            else if (relativeUrl.StartsWith("../"))
            {
                var parentDirectory = Directory.GetParent(baseDirectory)?.FullName ?? baseDirectory;
                var fileName = relativeUrl.Substring(3);
                return Path.Combine(parentDirectory, fileName);
            }
            else if (Path.IsPathRooted(relativeUrl))
            {
                return NormalizeFilePath(relativeUrl);
            }
            else
            {
                return Path.Combine(baseDirectory, relativeUrl);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to resolve URL: {relativeUrl} relative to {baseUrl}", ex);
        }
    }

    private static string NormalizeFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            path = path.Substring(7);

        path = path.Replace('/', Path.DirectorySeparatorChar);

        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return path;
        }
    }
}