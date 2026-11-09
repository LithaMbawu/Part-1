using CybersecurityChatbot.Core;
using Xunit;

namespace CybersecurityChatbot.Core.Tests;

public class TaskRequestParserTests
{
    private static readonly DateTime Today = new(2026, 11, 4);

    [Theory]
    [InlineData("Remind me to update my password tomorrow", "update my password", "2026-11-05")]
    [InlineData("Could you add a task to enable MFA in three days?", "enable MFA", "2026-11-07")]
    [InlineData("I need to back up my phone next week", "back up my phone", "2026-11-11")]
    [InlineData("Please create a reminder to review privacy settings in 2 months", "review privacy settings", "2027-01-04")]
    [InlineData("Task: update my recovery email on 2026-11-10", "update my recovery email", "2026-11-10")]
    [InlineData("Don't let me forget to update my browser today", "update my browser", "2026-11-04")]
    public void TryParse_RecognizesNaturalPhrasingAndReminderDates(string input, string expectedTitle, string expectedDate)
    {
        var parsed = TaskRequestParser.TryParse(input, Today, out var request);

        Assert.True(parsed);
        Assert.Equal(expectedTitle, request.Title);
        Assert.Equal(DateTime.Parse(expectedDate).Date, request.ReminderAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("What is phishing?")]
    [InlineData("I need help with password safety")]
    [InlineData("Remind me to")]
    public void TryParse_RejectsMessagesThatAreNotUsableTaskRequests(string input)
    {
        Assert.False(TaskRequestParser.TryParse(input, Today, out _));
    }

    [Fact]
    public void TryParse_LimitsTaskTitlesToDatabaseColumnLength()
    {
        var input = $"Add a task to {new string('x', 240)}";

        Assert.True(TaskRequestParser.TryParse(input, Today, out var request));

        Assert.Equal(200, request.Title.Length);
    }
}
