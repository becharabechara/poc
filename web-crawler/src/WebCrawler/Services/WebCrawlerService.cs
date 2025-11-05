using WebCrawler.Interfaces;
using WebCrawler.Models;

namespace WebCrawler.Services;

public sealed class WebCrawlerService : IWebCrawlerService
{
    private readonly IWebBrowser _webBrowser;
    private readonly IEmailExtractor _emailExtractor;

    public WebCrawlerService(IWebBrowser webBrowser, IEmailExtractor emailExtractor)
    {
        _webBrowser = webBrowser ?? throw new ArgumentNullException(nameof(webBrowser));
        _emailExtractor = emailExtractor ?? throw new ArgumentNullException(nameof(emailExtractor));
    }

    public async Task<CrawlResult> CrawlAsync(string startUrl, int maxDepth = 2)
    {
        if (string.IsNullOrWhiteSpace(startUrl))
            throw new ArgumentException("Start URL cannot be null or empty", nameof(startUrl));
        
        if (maxDepth < 0)
            throw new ArgumentException("Max depth cannot be negative", nameof(maxDepth));

        var visitedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var allEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var crawledPages = new List<PageInfo>();
        var queue = new Queue<(string url, int depth)>();
        
        queue.Enqueue((startUrl, 0));

        while (queue.Count > 0)
        {
            var (currentUrl, currentDepth) = queue.Dequeue();

            if (visitedUrls.Contains(currentUrl) || currentDepth > maxDepth)
                continue;

            try
            {
                visitedUrls.Add(currentUrl);
                var content = await _webBrowser.GetContentAsync(currentUrl);
                var pageEmails = _emailExtractor.ExtractEmails(content);
                var links = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                if (currentDepth < maxDepth)
                {
                    var extractedLinks = _emailExtractor.ExtractLinks(content);
                    
                    foreach (var link in extractedLinks)
                    {
                        try
                        {
                            var resolvedUrl = _webBrowser.ResolveUrl(currentUrl, link);
                            
                            if (!visitedUrls.Contains(resolvedUrl))
                            {
                                links.Add(resolvedUrl);
                                queue.Enqueue((resolvedUrl, currentDepth + 1));
                            }
                        }
                        catch
                        {
                            // Skip invalid URLs
                        }
                    }
                }

                foreach (var email in pageEmails)
                    allEmails.Add(email);

                crawledPages.Add(new PageInfo
                {
                    Url = currentUrl,
                    Depth = currentDepth,
                    Emails = pageEmails.ToHashSet(StringComparer.OrdinalIgnoreCase),
                    Links = links
                });
            }
            catch (Exception ex)
            {
                crawledPages.Add(new PageInfo
                {
                    Url = currentUrl,
                    Depth = currentDepth,
                    Emails = new HashSet<string>(),
                    Links = new HashSet<string>(),
                    Error = ex.Message
                });
            }
        }

        return new CrawlResult
        {
            StartUrl = startUrl,
            MaxDepth = maxDepth,
            TotalPages = crawledPages.Count,
            AllEmails = allEmails,
            Pages = crawledPages
        };
    }
}