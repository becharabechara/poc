using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IWebCrawlerService
{
    Task<CrawlResult> CrawlAsync(string startUrl, int maxDepth);
}