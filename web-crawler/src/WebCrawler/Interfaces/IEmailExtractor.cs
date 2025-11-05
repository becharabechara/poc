namespace WebCrawler.Interfaces;

public interface IEmailExtractor
{
    ISet<string> ExtractEmails(string htmlContent);
    ISet<string> ExtractLinks(string htmlContent);
}