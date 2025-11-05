namespace WebCrawler.Models;

public sealed class CrawlResult
{
    public string StartUrl { get; set; } = string.Empty;
    public int MaxDepth { get; set; }
    public int TotalPages { get; set; }
    public ISet<string> AllEmails { get; set; } = new HashSet<string>();
    public IList<PageInfo> Pages { get; set; } = new List<PageInfo>();
}