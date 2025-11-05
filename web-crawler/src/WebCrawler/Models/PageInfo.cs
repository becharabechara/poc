namespace WebCrawler.Models;

public sealed class PageInfo
{
    public string Url { get; set; } = string.Empty;
    public int Depth { get; set; }
    public ISet<string> Emails { get; set; } = new HashSet<string>();
    public ISet<string> Links { get; set; } = new HashSet<string>();
    public string? Error { get; set; }
}