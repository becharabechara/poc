namespace WebCrawler.Interfaces;

public interface IWebBrowser
{
    Task<string> GetContentAsync(string url);
    string ResolveUrl(string baseUrl, string relativeUrl);
}