using System.Globalization;
using System.Text.RegularExpressions;

namespace CybersecurityChatbot.Core;

public sealed record TaskRequest(string Title, DateTime? ReminderAt);

/// <summary>Recognizes common task and reminder phrasing without requiring one exact command.</summary>
public static partial class TaskRequestParser
{
    private static readonly Regex RequestPattern = new(
        @"^(?:(?:please\s+)?(?:(?:can|could|would)\s+you\s+)?(?:add|create|make)\s+(?:(?:me|my)\s+)?(?:a\s+)?(?:(?:cybersecurity|security)\s+)?(?:task|reminder|to[- ]?do)\b\s*(?:to|for)?\s*|(?:please\s+)?(?:(?:can|could|would)\s+you\s+)?set\s+(?:(?:up)\s+)?(?:a\s+)?(?:(?:cybersecurity|security)\s+)?(?:task|reminder)\b\s*(?:to|for)?\s*|(?:please\s+)?(?:(?:can|could|would)\s+you\s+)?remind\s+me\s+(?:to|about)\s+|(?:please\s+)?(?:don't|do\s+not)\s+let\s+me\s+forget\s+to\s+|(?:please\s+)?remember\s+to\s+|(?:please\s+)?(?:i\s+need\s+to|i\s+should|i\s+have\s+to|i\s+must)\s+|(?:please\s+)?task\s*:\s*)(?<title>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex DateSuffixPattern = new(
        @"\s+(?:(?:today|tomorrow|next\s+week)|(?:in\s+(?:a|one|two|three|four|five|six|seven|eight|nine|ten|\d+)\s+(?:days?|weeks?|months?))|(?:on\s+\d{4}-\d{1,2}-\d{1,2}))\s*[.!?]*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static bool TryParse(string input, out TaskRequest request) =>
        TryParse(input, DateTime.Today, out request);

    public static bool TryParse(string input, DateTime today, out TaskRequest request)
    {
        request = new TaskRequest(string.Empty, null);
        if (string.IsNullOrWhiteSpace(input)) return false;

        var match = RequestPattern.Match(input.Trim());
        if (!match.Success) return false;

        var reminderAt = ParseReminderDate(input, today);
        var title = DateSuffixPattern.Replace(match.Groups["title"].Value, string.Empty)
            .Trim().TrimEnd('.', '!', '?');
        if (title.Length == 0) return false;

        request = new TaskRequest(title.Length > 200 ? title[..200] : title, reminderAt);
        return true;
    }

    public static DateTime? ParseReminderDate(string input, DateTime today)
    {
        if (Regex.IsMatch(input, @"\btoday\b", RegexOptions.IgnoreCase)) return today.Date;
        if (Regex.IsMatch(input, @"\btomorrow\b", RegexOptions.IgnoreCase)) return today.Date.AddDays(1);
        if (Regex.IsMatch(input, @"\bnext\s+week\b", RegexOptions.IgnoreCase)) return today.Date.AddDays(7);

        var interval = Regex.Match(input,
            @"\bin\s+(?<amount>a|one|two|three|four|five|six|seven|eight|nine|ten|\d+)\s+(?<unit>days?|weeks?|months?)\b",
            RegexOptions.IgnoreCase);
        if (interval.Success && TryParseAmount(interval.Groups["amount"].Value, out var amount) && amount is > 0 and <= 3650)
        {
            var unit = interval.Groups["unit"].Value;
            if (unit.StartsWith("week", StringComparison.OrdinalIgnoreCase)) return today.Date.AddDays(amount * 7);
            if (unit.StartsWith("month", StringComparison.OrdinalIgnoreCase)) return today.Date.AddMonths(amount);
            return today.Date.AddDays(amount);
        }

        var dateMatch = Regex.Match(input, @"\bon\s+(?<date>\d{4}-\d{1,2}-\d{1,2})\b", RegexOptions.IgnoreCase);
        return dateMatch.Success && DateTime.TryParseExact(dateMatch.Groups["date"].Value,
            new[] { "yyyy-M-d", "yyyy-MM-dd", "yyyy-M-dd", "yyyy-MM-d" }, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsedDate) ? parsedDate.Date : null;
    }

    private static bool TryParseAmount(string value, out int amount)
    {
        amount = value.ToLowerInvariant() switch
        {
            "a" or "one" => 1,
            "two" => 2,
            "three" => 3,
            "four" => 4,
            "five" => 5,
            "six" => 6,
            "seven" => 7,
            "eight" => 8,
            "nine" => 9,
            "ten" => 10,
            _ => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0
        };
        return amount > 0;
    }
}
