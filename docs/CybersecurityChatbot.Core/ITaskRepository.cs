namespace CybersecurityChatbot.Core;

public interface ITaskRepository
{
    Task InitializeAsync();
    Task<IReadOnlyList<CybersecurityTask>> GetAllAsync();
    Task<long> AddAsync(string title, string description, DateTime? reminderAt);
    Task SetCompletedAsync(long id, bool isCompleted);
    Task SetReminderAsync(long id, DateTime? reminderAt);
    Task DeleteAsync(long id);
}