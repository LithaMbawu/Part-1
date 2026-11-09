using CybersecurityChatbot.Core;
using Xunit;

namespace CybersecurityChatbot.Core.Tests;

public sealed class SqliteTaskRepositoryTests
{
    [Fact]
    public async Task TaskCrud_PersistsBetweenRepositoryInstances()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "tasks.db");
        try
        {
            var repository = new SqliteTaskRepository(databasePath);
            await repository.InitializeAsync();
            var reminder = new DateTime(2026, 12, 1);
            var id = await repository.AddAsync("Update recovery email", "Add a recovery address to the account.", reminder);

            var added = Assert.Single(await repository.GetAllAsync());
            Assert.Equal(id, added.Id);
            Assert.Equal("Update recovery email", added.Title);
            Assert.Equal(reminder, added.ReminderAt);
            Assert.False(added.IsCompleted);

            await repository.SetCompletedAsync(id, true);
            await repository.SetReminderAsync(id, null);

            var reopenedRepository = new SqliteTaskRepository(databasePath);
            await reopenedRepository.InitializeAsync();
            var updated = Assert.Single(await reopenedRepository.GetAllAsync());
            Assert.True(updated.IsCompleted);
            Assert.Null(updated.ReminderAt);

            await reopenedRepository.DeleteAsync(id);
            Assert.Empty(await reopenedRepository.GetAllAsync());
        }
        finally
        {
            var directory = Path.GetDirectoryName(databasePath);
            if (directory is not null && Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
