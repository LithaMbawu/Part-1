namespace CybersecurityChatbot.Core;

public class ChatbotMemory
{
    public string? UserName { get; private set; }
    public string? FavouriteTopic { get; private set; }
    public string? LastTopic { get; private set; }
    public string? LastSentiment { get; private set; }

    public void SaveUserName(string name)
    {
        UserName = name.Trim();
    }

    public void SaveFavouriteTopic(string topic)
    {
        FavouriteTopic = topic.Trim();
        LastTopic = topic.Trim();
    }

    public void TrackTopic(string topic)
    {
        if (!string.IsNullOrWhiteSpace(topic))
        {
            LastTopic = topic.Trim();
        }
    }

    public void TrackSentiment(string sentiment)
    {
        if (!string.IsNullOrWhiteSpace(sentiment))
        {
            LastSentiment = sentiment.Trim();
        }
    }
}
