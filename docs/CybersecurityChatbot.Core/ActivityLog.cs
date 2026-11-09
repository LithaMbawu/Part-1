namespace CybersecurityChatbot.Core;

public sealed record ActivityLogEntry(DateTime Timestamp, string Category, string Description)
{
    public string DisplayText => $"{Timestamp:yyyy-MM-dd HH:mm}  ·  {Category}  ·  {Description}";
}

/// <summary>Maintains a newest-first, bounded activity history with user-controlled paging.</summary>
public sealed class ActivityLog(int maximumEntries = 200, int pageSize = 10)
{
    private readonly List<ActivityLogEntry> _entries = new();
    private readonly int _maximumEntries = Math.Max(10, maximumEntries);
    private readonly int _pageSize = Math.Max(1, pageSize);
    private int _visibleCount;

    public int TotalCount => _entries.Count;
    public int VisibleCount => Math.Min(_visibleCount, _entries.Count);
    public bool HasMore => VisibleCount < TotalCount;
    public IReadOnlyList<ActivityLogEntry> VisibleEntries => _entries.Take(VisibleCount).ToArray();

    public void Add(string category, string description, DateTime? timestamp = null)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("A category is required.", nameof(category));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("A description is required.", nameof(description));

        _entries.Insert(0, new ActivityLogEntry(timestamp ?? DateTime.Now, category.Trim(), description.Trim()));
        if (_entries.Count > _maximumEntries) _entries.RemoveRange(_maximumEntries, _entries.Count - _maximumEntries);
        _visibleCount = Math.Min(Math.Max(_visibleCount, _pageSize), _entries.Count);
    }

    public IReadOnlyList<ActivityLogEntry> ShowMore()
    {
        _visibleCount = Math.Min(_visibleCount + _pageSize, _entries.Count);
        return VisibleEntries;
    }

    public IReadOnlyList<ActivityLogEntry> ShowAll()
    {
        _visibleCount = _entries.Count;
        return VisibleEntries;
    }
}
