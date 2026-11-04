namespace CybersecurityChatbot.Core;

public sealed record CybersecurityTask(
    long Id,
    string Title,
    string Description,
    DateTime? ReminderAt,
    bool IsCompleted,
    DateTime CreatedAt);