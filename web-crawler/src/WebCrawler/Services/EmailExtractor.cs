using System.Text.RegularExpressions;
using WebCrawler.Interfaces;

namespace WebCrawler.Services;

public sealed class EmailExtractor : IEmailExtractor
{
    private static readonly Regex MailtoRegex = new(
        @"href\s*=\s*[""']mailto:([^""'>\s]+)[""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HrefRegex = new(
        @"href\s*=\s*[""']([^""'>\s]+)[""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EmailRegex = new(
        @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ISet<string> ExtractEmails(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
            return new HashSet<string>();

        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var mailtoMatches = MailtoRegex.Matches(htmlContent);
        foreach (Match match in mailtoMatches)
        {
            var email = match.Groups[1].Value;
            if (IsValidEmail(email))
                emails.Add(email);
        }

        var hrefMatches = HrefRegex.Matches(htmlContent);
        foreach (Match match in hrefMatches)
        {
            var hrefValue = match.Groups[1].Value;
            var emailMatches = EmailRegex.Matches(hrefValue);
            foreach (Match emailMatch in emailMatches)
            {
                var email = emailMatch.Value;
                if (IsValidEmail(email))
                    emails.Add(email);
            }
        }

        return emails;
    }

    public ISet<string> ExtractLinks(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent))
            return new HashSet<string>();

        var links = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var matches = HrefRegex.Matches(htmlContent);
        
        foreach (Match match in matches)
        {
            var href = match.Groups[1].Value;

            if (!string.IsNullOrWhiteSpace(href) &&
                !href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) &&
                !href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !href.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase) &&
                !href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) &&
                !href.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                links.Add(href);
            }
        }

        return links;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var atIndex = email.IndexOf('@');
        return atIndex > 0 && atIndex < email.Length - 1;
    }
}