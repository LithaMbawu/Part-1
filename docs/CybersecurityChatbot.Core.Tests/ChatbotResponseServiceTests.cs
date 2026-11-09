using CybersecurityChatbot.Core;
using Xunit;

namespace CybersecurityChatbot.Core.Tests;

public class ChatbotResponseServiceTests
{
    [Theory]
    [InlineData("password", "password")]
    [InlineData("scam", "scam")]
    [InlineData("privacy", "privacy")]
    [InlineData("phishing", "phishing")]
    [InlineData("malware", "malware")]
    public void GetResponse_RecognizesSupportedTopics(string input, string topic)
    {
        var service = new ChatbotResponseService();
        using var fixture = new TemporaryMemory();
        var memory = fixture.Memory;

        var response = service.GetResponse(input, memory);

        Assert.False(string.IsNullOrWhiteSpace(response));
        Assert.Equal(topic, memory.LastTopic);
    }

    [Fact]
    public void GetResponse_AdaptsResponseToWorriedSentiment()
    {
        var service = new ChatbotResponseService();
        using var fixture = new TemporaryMemory();
        var memory = fixture.Memory;

        var response = service.GetResponse("I am worried about phishing", memory);

        Assert.StartsWith("It’s completely understandable to feel that way.", response);
        Assert.Equal("worried", memory.LastSentiment);
        Assert.Equal("phishing", memory.LastTopic);
        Assert.Null(memory.UserName);
    }

    [Fact]
    public void GetResponse_ContinuesPreviousTopic()
    {
        var service = new ChatbotResponseService();
        using var fixture = new TemporaryMemory();
        var memory = fixture.Memory;
        service.GetResponse("phishing", memory);

        var response = service.GetResponse("tell me more", memory);

        Assert.Contains("keep talking about phishing", response);
    }

    [Theory]
    [InlineData("How can I protect my passphrases?", "password")]
    [InlineData("What is ransomware?", "malware")]
    [InlineData("Should I turn on 2FA?", "multi-factor authentication")]
    [InlineData("Tell me about social engineering", "social engineering")]
    [InlineData("How do backups help?", "backup")]
    [InlineData("Is public Wi-Fi safe?", "wifi")]
    public void GetResponse_RecognizesTopicAliases(string input, string expectedTopic)
    {
        var service = new ChatbotResponseService();
        using var fixture = new TemporaryMemory();

        var response = service.GetResponse(input, fixture.Memory);

        Assert.False(string.IsNullOrWhiteSpace(response));
        Assert.Equal(expectedTopic, fixture.Memory.LastTopic);
    }

    [Fact]
    public void GetResponse_OffersContextualClarificationForAmbiguousPhrase()
    {
        var service = new ChatbotResponseService();
        using var fixture = new TemporaryMemory();

        var response = service.GetResponse("I got a strange link", fixture.Memory);

        Assert.Contains("suspicious messages and phishing", response);
    }

    [Fact]
    public void GetResponse_UsesSavedNameInGreeting()
    {
        var service = new ChatbotResponseService();
        using var fixture = new TemporaryMemory();
        fixture.Memory.SaveUserName("Ada");

        var response = service.GetResponse("hello", fixture.Memory);

        Assert.Contains("Hello, Ada", response);
    }

    [Fact]
    public void ChatbotMemory_RestoresPersonalDetailsFromDisk()
    {
        using var fixture = new TemporaryMemory();
        fixture.Memory.SaveUserName("Ada");
        fixture.Memory.SaveFavouriteTopic("privacy");
        fixture.Memory.TrackSentiment("curious");

        var nextSession = new ChatbotMemory(fixture.MemoryPath);

        Assert.Equal("Ada", nextSession.UserName);
        Assert.Equal("privacy", nextSession.FavouriteTopic);
        Assert.Equal("privacy", nextSession.LastTopic);
        Assert.Equal("curious", nextSession.LastSentiment);
    }

    [Fact]
    public void QuizService_ContainsMoreThanTenQuestions()
    {
        var quiz = new QuizService();

        quiz.Start();

        Assert.True(quiz.TotalQuestions > 10);
        Assert.NotNull(quiz.CurrentQuestion);
        Assert.Equal(4, quiz.CurrentQuestion.Options.Count);
    }

    [Fact]
    public void QuizService_TracksCorrectAndIncorrectAnswersWithFeedback()
    {
        var quiz = new QuizService();
        quiz.Start();

        var correct = quiz.SubmitAnswer(1);

        Assert.True(correct.IsCorrect);
        Assert.Equal(1, correct.CorrectAnswers);
        Assert.Contains("password", correct.Explanation, StringComparison.OrdinalIgnoreCase);
        Assert.True(quiz.MoveNext());

        var incorrect = quiz.SubmitAnswer(0);

        Assert.False(incorrect.IsCorrect);
        Assert.Equal(1, incorrect.CorrectAnswers);
        Assert.False(string.IsNullOrWhiteSpace(incorrect.Explanation));
    }

    [Fact]
    public void QuizService_DoesNotAllowAnsweringAQuestionTwice()
    {
        var quiz = new QuizService();
        quiz.Start();
        quiz.SubmitAnswer(1);

        Assert.Throws<InvalidOperationException>(() => quiz.SubmitAnswer(0));
    }

    private sealed class TemporaryMemory : IDisposable
    {
        public string MemoryPath { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "memory.json");

        public ChatbotMemory Memory { get; }

        public TemporaryMemory()
        {
            Memory = new ChatbotMemory(MemoryPath);
        }

        public void Dispose()
        {
            var directory = Path.GetDirectoryName(MemoryPath);
            if (directory is not null && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
