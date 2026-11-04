using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CybersecurityChatbot.Core;
using CybersecurityChatbot.Gui;

namespace CybersecurityChatbot.Desktop;

public partial class MainWindow : Window
{
    private readonly ChatbotResponseService _chatbot = new();
    private readonly ChatbotMemory _memory = new();
    private readonly QuizService _quiz = new();
    private readonly ObservableCollection<ChatMessage> _messages = new();
    private readonly ObservableCollection<TaskViewModel> _tasks = new();
    private readonly ObservableCollection<string> _activity = new();
    private MySqlTaskRepository? _taskRepository;
    private long? _pendingReminderTaskId;
    private bool _quizAnswered;

    public MainWindow()
    {
        InitializeComponent();
        ChatItems.ItemsSource = _messages;
        TaskItems.ItemsSource = _tasks;
        ActivityItems.ItemsSource = _activity;
        AddBotMessage("Welcome! I can help with cybersecurity advice, task reminders, and a short security quiz. Try ‘add a task to enable 2FA’, ‘start quiz’, or ‘show activity log’.");
        Opened += async (_, _) => await InitializeTaskStorageAsync();
    }

    private async Task InitializeTaskStorageAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("CYBERSECURITY_CHATBOT_MYSQL");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            DatabaseStatus.Text = "MySQL is not configured. Set CYBERSECURITY_CHATBOT_MYSQL to a connection string; task storage is unavailable until configured.";
            AddTaskButton.IsEnabled = false;
            return;
        }

        try
        {
            _taskRepository = new MySqlTaskRepository(connectionString);
            await _taskRepository.InitializeAsync();
            await ReloadTasksAsync();
            DatabaseStatus.Text = "Connected to MySQL. Tasks and reminders are saved in the database.";
            AddTaskButton.IsEnabled = true;
        }
        catch (Exception exception) when (exception is MySqlConnector.MySqlException or IOException or InvalidOperationException)
        {
            _taskRepository = null;
            DatabaseStatus.Text = "Could not connect to MySQL. Check that the server/database exists and the connection string is valid.";
            AddTaskButton.IsEnabled = false;
        }
    }

    private async void Send_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await HandleUserInputAsync();

    private async void UserInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await HandleUserInputAsync();
        }
    }

    private async Task HandleUserInputAsync()
    {
        var input = UserInput.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(input)) return;

        AddUserMessage(input);
        UserInput.Clear();

        if (ContainsAny(input, "show activity log", "activity log", "what have you done for me", "recent actions"))
        {
            LogAction("Activity log viewed (NLP command recognized).");
            AddBotMessage(FormatActivityLog());
            return;
        }

        if (ContainsAny(input, "start quiz", "start the quiz", "cyber quiz", "quiz game", "take a quiz"))
        {
            StartQuiz();
            MainTabs.SelectedIndex = 2;
            AddBotMessage("Quiz started. Answer one question at a time; I’ll explain each answer straight away.");
            return;
        }

        if (_pendingReminderTaskId.HasValue && ContainsAny(input, "yes", "remind me", "set a reminder", "in "))
        {
            var reminderAt = ParseReminderDate(input);
            if (reminderAt.HasValue)
            {
                await SetPendingReminderAsync(reminderAt.Value);
                return;
            }
        }

        if (TryExtractTask(input, out var title, out var reminderDate))
        {
            LogAction("NLP interpreted a task/reminder request.");
            await AddTaskFromChatAsync(title, reminderDate);
            return;
        }

        AddBotMessage(_chatbot.GetResponse(input, _memory));
    }

    private async Task AddTaskFromChatAsync(string title, DateTime? reminderAt)
    {
        if (_taskRepository is null)
        {
            AddBotMessage("I understood that as a task request, but task storage is unavailable until MySQL is configured.");
            return;
        }

        title = title.Trim().TrimEnd('.', '!', '?');
        if (title.Length > 200) title = title[..200];
        if (string.IsNullOrWhiteSpace(title))
        {
            AddBotMessage("Please include a short task description, for example: ‘Add a task to enable two-factor authentication.’");
            return;
        }

        try
        {
            var id = await _taskRepository.AddAsync(title, $"Cybersecurity task: {title}.", reminderAt);
            await ReloadTasksAsync();
            LogAction(reminderAt.HasValue ? $"Task added: '{title}' (reminder {reminderAt.Value:g})." : $"Task added: '{title}' (no reminder set).");
            if (reminderAt.HasValue)
                AddBotMessage($"Task added: '{title}'. Reminder set for {reminderAt.Value:g}.");
            else
            {
                _pendingReminderTaskId = id;
                AddBotMessage($"Task added: '{title}'. Would you like a reminder? You can say ‘yes, remind me in 3 days’.");
            }
        }
        catch (MySqlConnector.MySqlException)
        {
            AddBotMessage("I couldn't save that task to MySQL. Check your database connection and try again.");
        }
    }

    private async Task SetPendingReminderAsync(DateTime reminderAt)
    {
        if (_taskRepository is null || !_pendingReminderTaskId.HasValue) return;
        try
        {
            var task = _tasks.FirstOrDefault(item => item.Id == _pendingReminderTaskId.Value);
            var title = task?.Title ?? "your task";
            await _taskRepository.SetReminderAsync(_pendingReminderTaskId.Value, reminderAt);
            _pendingReminderTaskId = null;
            await ReloadTasksAsync();
            LogAction($"Reminder set for '{title}' on {reminderAt:g}.");
            AddBotMessage($"Got it! I'll remind you about '{title}' on {reminderAt:g}.");
        }
        catch (MySqlConnector.MySqlException)
        {
            AddBotMessage("I couldn't save that reminder. Please check the MySQL connection and try again.");
        }
    }

    private async void AddTask_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var title = TaskTitle.Text?.Trim() ?? string.Empty;
        var description = TaskDescription.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
        {
            await ShowMessageAsync("Enter both a task title and description.");
            return;
        }
        if (_taskRepository is null)
        {
            await ShowMessageAsync("MySQL is not connected. Configure the database before saving tasks.");
            return;
        }
        try
        {
            await _taskRepository.AddAsync(title, description, null);
            await ReloadTasksAsync();
            LogAction($"Task added: '{title}'.");
            AddBotMessage($"Task added: '{title}'. Would you like a reminder?");
            TaskTitle.Clear();
            TaskDescription.Clear();
        }
        catch (MySqlConnector.MySqlException)
        {
            await ShowMessageAsync("The task could not be saved. Check the MySQL connection.");
        }
    }

    private async void ToggleTask_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TaskItems.SelectedItem is not TaskViewModel task || _taskRepository is null) return;
        try
        {
            await _taskRepository.SetCompletedAsync(task.Id, !task.IsCompleted);
            await ReloadTasksAsync();
            var action = task.IsCompleted ? "reopened" : "marked complete";
            LogAction($"Task '{task.Title}' {action}.");
            AddBotMessage($"Task '{task.Title}' {action}.");
        }
        catch (MySqlConnector.MySqlException) { await ShowMessageAsync("The task could not be updated in MySQL."); }
    }

    private async void DeleteTask_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TaskItems.SelectedItem is not TaskViewModel task || _taskRepository is null) return;
        if (!await ConfirmDeleteAsync($"Delete '{task.Title}'?")) return;
        try
        {
            await _taskRepository.DeleteAsync(task.Id);
            await ReloadTasksAsync();
            LogAction($"Task deleted: '{task.Title}'.");
            AddBotMessage($"Task '{task.Title}' was deleted.");
        }
        catch (MySqlConnector.MySqlException) { await ShowMessageAsync("The task could not be deleted from MySQL."); }
    }

    private async Task ReloadTasksAsync()
    {
        if (_taskRepository is null) return;
        var loadedTasks = await _taskRepository.GetAllAsync();
        _tasks.Clear();
        foreach (var task in loadedTasks) _tasks.Add(new TaskViewModel(task));
    }

    private void StartQuiz()
    {
        _quiz.Start();
        _quizAnswered = false;
        QuizFeedback.Text = string.Empty;
        QuizScore.Text = "Score: 0 / 12";
        LogAction("Cybersecurity quiz started (12 questions).");
        RenderCurrentQuestion();
    }

    private void StartQuiz_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => StartQuiz();

    private void RenderCurrentQuestion()
    {
        var question = _quiz.CurrentQuestion;
        if (question is null) return;
        QuizQuestion.Text = $"Question {_quiz.QuestionsAnswered + 1} of {_quiz.TotalQuestions}: {question.Prompt}";
        QuizProgress.Text = $"Question {_quiz.QuestionsAnswered + 1} / {_quiz.TotalQuestions}";
        QuizOptions.Children.Clear();
        for (var index = 0; index < question.Options.Count; index++)
        {
            var option = new RadioButton { Content = question.Options[index], Tag = index, GroupName = "CyberQuizOption", Margin = new Thickness(0, 0, 0, 8) };
            option.IsCheckedChanged += (_, _) => SubmitAnswerButton.IsEnabled = !_quizAnswered && QuizOptions.Children.OfType<RadioButton>().Any(item => item.IsChecked == true);
            QuizOptions.Children.Add(option);
        }
        SubmitAnswerButton.IsEnabled = false;
        NextQuestionButton.IsEnabled = false;
        NextQuestionButton.Content = "Next question";
    }

    private void SubmitAnswer_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selectedOption = QuizOptions.Children.OfType<RadioButton>().FirstOrDefault(option => option.IsChecked == true);
        if (selectedOption?.Tag is not int answerIndex) return;
        var result = _quiz.SubmitAnswer(answerIndex);
        _quizAnswered = true;
        SubmitAnswerButton.IsEnabled = false;
        foreach (var option in QuizOptions.Children.OfType<RadioButton>()) option.IsEnabled = false;
        QuizFeedback.Text = $"{(result.IsCorrect ? "Correct!" : "Not quite.")} {result.Explanation}";
        QuizFeedback.Foreground = new SolidColorBrush(result.IsCorrect ? Color.FromRgb(134, 239, 172) : Color.FromRgb(253, 186, 116));
        QuizScore.Text = $"Score: {result.CorrectAnswers} / {result.QuestionsAnswered}";
        NextQuestionButton.Content = result.QuestionsAnswered == _quiz.TotalQuestions ? "Finish quiz" : "Next question";
        NextQuestionButton.IsEnabled = true;
    }

    private void NextQuestion_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_quiz.MoveNext())
        {
            _quizAnswered = false;
            QuizFeedback.Text = string.Empty;
            RenderCurrentQuestion();
            QuizScore.Text = $"Score: {_quiz.CorrectAnswers} / {_quiz.QuestionsAnswered}";
            return;
        }
        var total = _quiz.TotalQuestions;
        var summary = _quiz.CorrectAnswers >= 10 ? "Great job! You're a cybersecurity pro!" : _quiz.CorrectAnswers >= 7 ? "Nice work. Keep practising these safe habits." : "Keep learning to stay safe online!";
        QuizQuestion.Text = $"Quiz complete — {_quiz.CorrectAnswers} / {total}. {summary}";
        QuizProgress.Text = "You completed all 12 questions.";
        QuizOptions.Children.Clear();
        QuizFeedback.Text = "Start the quiz again any time to practise.";
        QuizScore.Text = $"Final score: {_quiz.CorrectAnswers} / {total}";
        NextQuestionButton.IsEnabled = false;
        SubmitAnswerButton.IsEnabled = false;
        LogAction($"Cybersecurity quiz completed: {_quiz.CorrectAnswers}/{total} correct.");
        AddBotMessage($"Quiz complete: {_quiz.CorrectAnswers} out of {total}. {summary}");
    }

    private void LogAction(string description)
    {
        _activity.Insert(0, $"{DateTime.Now:yyyy-MM-dd HH:mm} — {description}");
        while (_activity.Count > 10) _activity.RemoveAt(_activity.Count - 1);
    }

    private string FormatActivityLog() => _activity.Count == 0
        ? "There are no recorded actions yet. Add a task or start the quiz to begin your activity log."
        : "Here's a summary of recent actions:\n" + string.Join("\n", _activity.Select((entry, index) => $"{index + 1}. {entry}"));

    private static bool TryExtractTask(string input, out string title, out DateTime? reminderAt)
    {
        title = string.Empty;
        reminderAt = ParseReminderDate(input);
        var match = Regex.Match(input,
            @"^(?:(?:please\s+)?(?:can\s+you\s+)?(?:add|create|make)\s+(?:a\s+)?(?:cybersecurity\s+)?(?:task|reminder)\s*(?:to\s+)?|(?:please\s+)?(?:can\s+you\s+)?set\s+(?:(?:up\s+)?(?:a\s+)?(?:cybersecurity\s+)?(?:task|reminder))\s*(?:to\s+)?|(?:please\s+)?(?:can\s+you\s+)?remind\s+me\s+(?:to|about)\s+|(?:please\s+)?remember\s+to\s+)(?<title>.+)$",
            RegexOptions.IgnoreCase);
        if (!match.Success) return false;
        title = Regex.Replace(match.Groups["title"].Value,
            @"\s+(?:(?:tomorrow)|(?:in\s+\d+\s+(?:days?|weeks?))|(?:on\s+\d{4}-\d{1,2}-\d{1,2}))\s*[.!?]*$",
            string.Empty, RegexOptions.IgnoreCase).Trim().TrimEnd('.', '!', '?');
        return title.Length > 0;
    }

    private static DateTime? ParseReminderDate(string input)
    {
        if (Regex.IsMatch(input, @"\btomorrow\b", RegexOptions.IgnoreCase)) return DateTime.Today.AddDays(1);
        var interval = Regex.Match(input, @"\bin\s+(?<amount>\d+)\s+(?<unit>days?|weeks?)\b", RegexOptions.IgnoreCase);
        if (interval.Success && int.TryParse(interval.Groups["amount"].Value, out var amount) && amount is > 0 and <= 3650)
        {
            var days = interval.Groups["unit"].Value.StartsWith("week", StringComparison.OrdinalIgnoreCase) ? amount * 7 : amount;
            return DateTime.Today.AddDays(days);
        }
        var dateMatch = Regex.Match(input, @"\bon\s+(?<date>\d{4}-\d{1,2}-\d{1,2})\b", RegexOptions.IgnoreCase);
        return dateMatch.Success && DateTime.TryParse(dateMatch.Groups["date"].Value, out var parsedDate) ? parsedDate.Date : null;
    }

    private static bool ContainsAny(string input, params string[] phrases) =>
        phrases.Any(phrase => input.Contains(phrase, StringComparison.OrdinalIgnoreCase));

    private void AddUserMessage(string message) => AddMessage(new ChatMessage("You", message, true));
    private void AddBotMessage(string message) => AddMessage(new ChatMessage("Cyber Shield", message, false));

    private void AddMessage(ChatMessage message)
    {
        _messages.Add(message);
        ChatScrollViewer.ScrollToEnd();
    }

    private async Task ShowMessageAsync(string message)
    {
        var dialog = new Window
        {
            Title = "Cyber Shield Assistant",
            Width = 420,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(20), Spacing = 18,
                Children = { new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap }, new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right } }
            }
        };
        if (dialog.Content is StackPanel panel && panel.Children[1] is Button button) button.Click += (_, _) => dialog.Close();
        await dialog.ShowDialog(this);
    }

    private async Task<bool> ConfirmDeleteAsync(string message)
    {
        var result = false;
        var dialog = new Window
        {
            Title = "Confirm delete", Width = 400, SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(20), Spacing = 18,
                Children = { new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap }, new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Spacing = 8 } }
            }
        };
        var buttons = (StackPanel)((StackPanel)dialog.Content!).Children[1]!;
        var cancel = new Button { Content = "Cancel" };
        var delete = new Button { Content = "Delete", Background = new SolidColorBrush(Color.Parse("#B91C1C")) };
        cancel.Click += (_, _) => dialog.Close();
        delete.Click += (_, _) => { result = true; dialog.Close(); };
        buttons.Children.Add(cancel);
        buttons.Children.Add(delete);
        await dialog.ShowDialog(this);
        return result;
    }

    private sealed record ChatMessage(string Sender, string Content, bool IsFromUser)
    {
        public string Alignment => IsFromUser ? "Right" : "Left";
        public string BubbleColor => IsFromUser ? "#164E63" : "#1E293B";
    }

    private sealed record TaskViewModel(CybersecurityTask Task)
    {
        public long Id => Task.Id;
        public string Title => Task.Title;
        public string Description => Task.Description;
        public DateTime? ReminderAt => Task.ReminderAt;
        public bool IsCompleted => Task.IsCompleted;
        public string DisplayTitle => IsCompleted ? $"✓ {Title}" : Title;
        public string ReminderText => ReminderAt.HasValue ? $"Reminder: {ReminderAt.Value:g}" : "No reminder";
    }
}
