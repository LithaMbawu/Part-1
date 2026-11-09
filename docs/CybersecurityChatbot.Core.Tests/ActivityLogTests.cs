using CybersecurityChatbot.Core;
using Xunit;

namespace CybersecurityChatbot.Core.Tests;

public class ActivityLogTests
{
    [Fact]
    public void Add_ShowsTenNewestInitially_AndShowMorePagesThroughHistory()
    {
        var log = new ActivityLog(maximumEntries: 30, pageSize: 10);
        for (var index = 1; index <= 25; index++) log.Add("Task", $"Action {index}");

        Assert.Equal(10, log.VisibleCount);
        Assert.Equal("Action 25", log.VisibleEntries[0].Description);
        Assert.True(log.HasMore);

        var secondPage = log.ShowMore();
        Assert.Equal(20, secondPage.Count);
        Assert.True(log.HasMore);

        var lastPage = log.ShowMore();
        Assert.Equal(25, lastPage.Count);
        Assert.False(log.HasMore);
    }

    [Fact]
    public void Add_EnforcesMaximumHistorySize()
    {
        var log = new ActivityLog(maximumEntries: 10, pageSize: 5);
        for (var index = 1; index <= 12; index++) log.Add("Quiz", $"Action {index}");
        log.ShowAll();

        Assert.Equal(10, log.TotalCount);
        Assert.Equal("Action 12", log.VisibleEntries[0].Description);
        Assert.Equal("Action 3", log.VisibleEntries[^1].Description);
    }
}
