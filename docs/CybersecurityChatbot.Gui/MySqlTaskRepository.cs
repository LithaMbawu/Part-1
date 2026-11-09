using CybersecurityChatbot.Core;
using MySqlConnector;

namespace CybersecurityChatbot.Gui;

public sealed class MySqlTaskRepository(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public async Task InitializeAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS CybersecurityTasks (
                Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                Title VARCHAR(200) NOT NULL,
                Description VARCHAR(2000) NOT NULL,
                ReminderAt DATETIME NULL,
                IsCompleted BOOLEAN NOT NULL DEFAULT FALSE,
                CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                INDEX IX_CybersecurityTasks_IsCompleted (IsCompleted),
                INDEX IX_CybersecurityTasks_ReminderAt (ReminderAt)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<CybersecurityTask>> GetAllAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, ReminderAt, IsCompleted, CreatedAt FROM CybersecurityTasks ORDER BY IsCompleted, ReminderAt IS NULL, ReminderAt, CreatedAt DESC";
        await using var reader = await command.ExecuteReaderAsync();
        var idOrdinal = reader.GetOrdinal("Id");
        var titleOrdinal = reader.GetOrdinal("Title");
        var descriptionOrdinal = reader.GetOrdinal("Description");
        var reminderOrdinal = reader.GetOrdinal("ReminderAt");
        var completedOrdinal = reader.GetOrdinal("IsCompleted");
        var createdOrdinal = reader.GetOrdinal("CreatedAt");
        var tasks = new List<CybersecurityTask>();
        while (await reader.ReadAsync())
        {
            tasks.Add(new CybersecurityTask(
            reader.GetInt64(idOrdinal),
            reader.GetString(titleOrdinal),
            reader.GetString(descriptionOrdinal),
            reader.IsDBNull(reminderOrdinal) ? null : reader.GetDateTime(reminderOrdinal),
            reader.GetBoolean(completedOrdinal),
            reader.GetDateTime(createdOrdinal)));
        }

        return tasks;
    }

    public async Task<long> AddAsync(string title, string description, DateTime? reminderAt)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO CybersecurityTasks (Title, Description, ReminderAt) VALUES (@title, @description, @reminderAt); SELECT LAST_INSERT_ID();";
        command.Parameters.AddWithValue("@title", title);
        command.Parameters.AddWithValue("@description", description);
        command.Parameters.AddWithValue("@reminderAt", reminderAt is null ? DBNull.Value : reminderAt.Value);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public async Task SetCompletedAsync(long id, bool isCompleted)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE CybersecurityTasks SET IsCompleted = @isCompleted WHERE Id = @id";
        command.Parameters.AddWithValue("@isCompleted", isCompleted);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task SetReminderAsync(long id, DateTime? reminderAt)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE CybersecurityTasks SET ReminderAt = @reminderAt WHERE Id = @id";
        command.Parameters.AddWithValue("@reminderAt", reminderAt is null ? DBNull.Value : reminderAt.Value);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM CybersecurityTasks WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }
}