using System.Globalization;
using CybersecurityChatbot.Core;
using Microsoft.Data.Sqlite;

namespace CybersecurityChatbot.Core;

public sealed class SqliteTaskRepository : ITaskRepository
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public string DatabasePath => _databasePath;

    public SqliteTaskRepository(string? databasePath = null)
    {
        _databasePath = Path.GetFullPath(databasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CybersecurityChatbot",
            "tasks.db"));
        _connectionString = new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString();
    }

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS CybersecurityTasks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL CHECK(length(Title) <= 200),
                Description TEXT NOT NULL CHECK(length(Description) <= 2000),
                ReminderAt TEXT NULL,
                IsCompleted INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_CybersecurityTasks_IsCompleted ON CybersecurityTasks (IsCompleted);
            CREATE INDEX IF NOT EXISTS IX_CybersecurityTasks_ReminderAt ON CybersecurityTasks (ReminderAt);
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<CybersecurityTask>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, ReminderAt, IsCompleted, CreatedAt FROM CybersecurityTasks ORDER BY IsCompleted, ReminderAt IS NULL, ReminderAt, CreatedAt DESC";
        await using var reader = await command.ExecuteReaderAsync();
        var tasks = new List<CybersecurityTask>();
        while (await reader.ReadAsync())
        {
            tasks.Add(new CybersecurityTask(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                reader.GetInt64(4) != 0,
                DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)));
        }

        return tasks;
    }

    public async Task<long> AddAsync(string title, string description, DateTime? reminderAt)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO CybersecurityTasks (Title, Description, ReminderAt, CreatedAt) VALUES (@title, @description, @reminderAt, @createdAt); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@title", title);
        command.Parameters.AddWithValue("@description", description);
        command.Parameters.AddWithValue("@reminderAt", reminderAt is null ? DBNull.Value : FormatDate(reminderAt.Value));
        command.Parameters.AddWithValue("@createdAt", FormatDate(DateTime.Now));
        return Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    public async Task SetCompletedAsync(long id, bool isCompleted)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE CybersecurityTasks SET IsCompleted = @isCompleted WHERE Id = @id";
        command.Parameters.AddWithValue("@isCompleted", isCompleted ? 1 : 0);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task SetReminderAsync(long id, DateTime? reminderAt)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE CybersecurityTasks SET ReminderAt = @reminderAt WHERE Id = @id";
        command.Parameters.AddWithValue("@reminderAt", reminderAt is null ? DBNull.Value : FormatDate(reminderAt.Value));
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM CybersecurityTasks WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    private static string FormatDate(DateTime value) => value.ToString("O", CultureInfo.InvariantCulture);
}