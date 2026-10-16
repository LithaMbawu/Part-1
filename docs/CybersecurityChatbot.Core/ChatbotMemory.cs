using System.Text.Json;

namespace CybersecurityChatbot.Core;

public class ChatbotMemory
{
    private readonly string _memoryFilePath;

    public string? UserName { get; private set; }
    public string? FavouriteTopic { get; private set; }
    public string? LastTopic { get; private set; }
    public string? LastSentiment { get; private set; }

    public ChatbotMemory(string? memoryFilePath = null)
    {
        _memoryFilePath = memoryFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CybersecurityChatbot",
            "memory.json");
        Load();
    }

    public void SaveUserName(string name)
    {
        UserName = name.Trim();
        Persist();
    }

    public void SaveFavouriteTopic(string topic)
    {
        FavouriteTopic = topic.Trim();
        LastTopic = topic.Trim();
        Persist();
    }

    public void TrackTopic(string topic)
    {
        if (!string.IsNullOrWhiteSpace(topic))
        {
            LastTopic = topic.Trim();
            Persist();
        }
    }

    public void TrackSentiment(string sentiment)
    {
        if (!string.IsNullOrWhiteSpace(sentiment))
        {
            LastSentiment = sentiment.Trim();
            Persist();
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_memoryFilePath))
            {
                return;
            }

            var saved = JsonSerializer.Deserialize<Dictionary<string, string?>>(File.ReadAllText(_memoryFilePath));
            if (saved is null)
            {
                return;
            }

            saved.TryGetValue(nameof(UserName), out var userName);
            saved.TryGetValue(nameof(FavouriteTopic), out var favouriteTopic);
            saved.TryGetValue(nameof(LastTopic), out var lastTopic);
            saved.TryGetValue(nameof(LastSentiment), out var lastSentiment);

            UserName = userName;
            FavouriteTopic = favouriteTopic;
            LastTopic = lastTopic;
            LastSentiment = lastSentiment;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            // Ignore unreadable or invalid saved memory so the chatbot remains usable.
        }
    }

    private void Persist()
    {
        var directory = Path.GetDirectoryName(_memoryFilePath);
        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        var saved = new Dictionary<string, string?>
        {
            [nameof(UserName)] = UserName,
            [nameof(FavouriteTopic)] = FavouriteTopic,
            [nameof(LastTopic)] = LastTopic,
            [nameof(LastSentiment)] = LastSentiment
        };
        var temporaryPath = $"{_memoryFilePath}.tmp";

        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(saved));
            File.Move(temporaryPath, _memoryFilePath, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Memory is optional; file-system issues must not interrupt the conversation.
            try
            {
                File.Delete(temporaryPath);
            }
            catch (Exception cleanupException) when (cleanupException is IOException or UnauthorizedAccessException)
            {
                // Best effort cleanup of a partial write.
            }
        }
    }
}
