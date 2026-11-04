using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbot.Core;

namespace CybersecurityChatbot.Gui;

public partial class MainWindow : Window
{
    private readonly ChatbotResponseService _chatbot = new();
    private readonly ChatbotMemory _memory = new();
    private readonly QuizService _quiz = new();
    private readonly ObservableCollection<ChatMessage> _messages = new();
    private readonly ObservableCollection<CybersecurityTask> _tasks = new();
    private readonly ObservableCollection<string> _activity = new();
    private MySqlTaskRepository? _taskRepository;
    private long? _pendingReminderTaskId;
    private bool _quizAnswered;

    public MainWindow()
    {
        InitializeComponent();
        ChatListBox.ItemsSource = _messages;
        TaskListDataGrid.ItemsSource = _tasks;
        ActivityListBox.ItemsSource = _activity;
        Loaded += MainWindow_Loaded;
        AddBotMessage("Welcome! I can help with cybersecurity advice, task reminders, and a short security quiz. Try “add a task to enable 2FA”, “start quiz”, or “show activity log”.");

        if (VoiceToggleButton.IsChecked == true)
        {
            PlayGreeting();
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var connectionString = Environment.GetEnvironmentVariable("CYBERSECURITY_CHATBOT_MYSQL");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            DatabaseStatusText.Text = "MySQL is not configured. Set CYBERSECURITY_CHATBOT_MYSQL to a connection string; see the README for setup. Task storage is unavailable until configured.";
            AddTaskButton.IsEnabled = false;
            return;
        }

        try
        {
            _taskRepository = new MySqlTaskRepository(connectionString);
            await _taskRepository.InitializeAsync();
            await ReloadTasksAsync();
            DatabaseStatusText.Text = "Connected to MySQL. Tasks and reminders are saved in the database.";
            AddTaskButton.IsEnabled = true;
        }
        catch (Exception exception) when (exception is MySqlConnector.MySqlException or IOException or InvalidOperationException)
        {
            _taskRepository = null;
            DatabaseStatusText.Text = "Could not connect to MySQL. Check that the server/database exists and the connection string is valid.";
            AddTaskButton.IsEnabled = false;
        }
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e) => await HandleUserInputAsync();

    private async void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await HandleUserInputAsync();
        }
    }

    private void VoiceToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var greetingEnabled = VoiceToggleButton.IsChecked == true;
        VoiceToggleButton.Content = greetingEnabled ? "🔊 Greeting on" : "🔇 Greeting off";
        if (greetingEnabled)
        {
            PlayGreeting();
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e) => Close();

    private async Task HandleUserInputAsync()
    {
        var input = UserInputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        AddUserMessage(input);
        UserInputTextBox.Clear();
        UserInputTextBox.Focus();

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

        try
        {
            title = title.Trim().TrimEnd('.', '!', '?');
            if (title.Length > 200)
            {
                title = title[..200];
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                AddBotMessage("Please include a short task description, for example: “Add a task to enable two-factor authentication.”");
                return;
            }

            var description = $"Cybersecurity task: {title}.";
            var id = await _taskRepository.AddAsync(title, description, reminderAt);
            await ReloadTasksAsync();
            LogAction(reminderAt.HasValue
                ? $"Task added: '{title}' (reminder {reminderAt.Value:g})."
                : $"Task added: '{title}' (no reminder set).");
            if (reminderAt.HasValue)
            {
                AddBotMessage($"Task added: '{title}'. Reminder set for {reminderAt.Value:g}.");
            }
            else
            {
                _pendingReminderTaskId = id;
                AddBotMessage($"Task added: '{title}'. Would you like a reminder? You can say “yes, remind me in 3 days”.");
            }
        }
        catch (MySqlConnector.MySqlException)
        {
            AddBotMessage("I couldn't save that task to MySQL. Check your database connection and try again.");
        }
    }

    private async Task SetPendingReminderAsync(DateTime reminderAt)
    {
        if (_taskRepository is null || !_pendingReminderTaskId.HasValue)
        {
            return;
        }

        try
        {
            await _taskRepository.SetReminderAsync(_pendingReminderTaskId.Value, reminderAt);
            var task = _tasks.FirstOrDefault(item => item.Id == _pendingReminderTaskId.Value);
            var title = task?.Title ?? "your task";
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

    private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TaskTitleTextBox.Text.Trim();
        var description = TaskDescriptionTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
        {
            MessageBox.Show("Enter both a task title and description.", "Task details required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_taskRepository is null)
        {
            MessageBox.Show("MySQL is not connected. Configure the database before saving tasks.", "Database unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var reminderAt = ReminderDatePicker.SelectedDate?.Date;
        if (reminderAt < DateTime.Today)
        {
            MessageBox.Show("Choose today or a future date for the reminder.", "Invalid reminder date", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            await _taskRepository.AddAsync(title, description, reminderAt);
            await ReloadTasksAsync();
            LogAction(reminderAt.HasValue ? $"Task added: '{title}' (reminder {reminderAt:yyyy-MM-dd})." : $"Task added: '{title}'.");
            AddBotMessage(reminderAt.HasValue ? $"Task added: '{title}'. Reminder set for {reminderAt:yyyy-MM-dd}." : $"Task added: '{title}'. Would you like a reminder?");
            TaskTitleTextBox.Clear();
            TaskDescriptionTextBox.Clear();
            ReminderDatePicker.SelectedDate = null;
        }
        catch (MySqlConnector.MySqlException)
        {
            MessageBox.Show("The task could not be saved. Check the MySQL connection.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ToggleTaskCompletedButton_Click(object sender, RoutedEventArgs e)
    {
        if (TaskListDataGrid.SelectedItem is not CybersecurityTask task || _taskRepository is null)
        {
            return;
        }

        try
        {
            await _taskRepository.SetCompletedAsync(task.Id, !task.IsCompleted);
            await ReloadTasksAsync();
            var action = task.IsCompleted ? "reopened" : "marked complete";
            LogAction($"Task '{task.Title}' {action}.");
            AddBotMessage($"Task '{task.Title}' {action}.");
        }
        catch (MySqlConnector.MySqlException)
        {
            MessageBox.Show("The task could not be updated in MySQL.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (TaskListDataGrid.SelectedItem is not CybersecurityTask task || _taskRepository is null)
        {
            return;
        }

        if (MessageBox.Show($"Delete '{task.Title}'?", "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _taskRepository.DeleteAsync(task.Id);
            await ReloadTasksAsync();
            LogAction($"Task deleted: '{task.Title}'.");
            AddBotMessage($"Task '{task.Title}' was deleted.");
        }
        catch (MySqlConnector.MySqlException)
        {
            MessageBox.Show("The task could not be deleted from MySQL.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ReloadTasksAsync()
    {
        if (_taskRepository is null)
        {
            return;
        }

        var loadedTasks = await _taskRepository.GetAllAsync();
        _tasks.Clear();
        foreach (var task in loadedTasks)
        {
            _tasks.Add(task);
        }
    }

    private void StartQuiz()
    {
        _quiz.Start();
        _quizAnswered = false;
        QuizFeedbackText.Text = string.Empty;
        QuizScoreText.Text = "Score: 0 / 12";
        LogAction("Cybersecurity quiz started (12 questions).");
        RenderCurrentQuestion();
    }

    private void StartQuizButton_Click(object sender, RoutedEventArgs e) => StartQuiz();

    private void RenderCurrentQuestion()
    {
        var question = _quiz.CurrentQuestion;
        if (question is null)
        {
            return;
        }

        QuizQuestionText.Text = $"Question {_quiz.QuestionsAnswered + 1} of {_quiz.TotalQuestions}: {question.Prompt}";
        QuizProgressText.Text = $"Question {_quiz.QuestionsAnswered + 1} / {_quiz.TotalQuestions}";
        QuizOptionsPanel.Children.Clear();
        for (var index = 0; index < question.Options.Count; index++)
        {
            var option = new RadioButton
            {
                Content = question.Options[index],
                Tag = index,
                GroupName = "CyberQuizOption",
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                FontSize = 15,
                Margin = new Thickness(0, 0, 0, 12)
            };
            option.Checked += (_, _) => SubmitQuizAnswerButton.IsEnabled = !_quizAnswered;
            QuizOptionsPanel.Children.Add(option);
        }

        SubmitQuizAnswerButton.IsEnabled = false;
        NextQuizQuestionButton.IsEnabled = false;
        NextQuizQuestionButton.Content = "Next question";
    }

    private void SubmitQuizAnswerButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedOption = QuizOptionsPanel.Children.OfType<RadioButton>().FirstOrDefault(option => option.IsChecked == true);
        if (selectedOption?.Tag is not int answerIndex)
        {
            return;
        }

        var result = _quiz.SubmitAnswer(answerIndex);
        _quizAnswered = true;
        SubmitQuizAnswerButton.IsEnabled = false;
        foreach (var option in QuizOptionsPanel.Children.OfType<RadioButton>())
        {
            option.IsEnabled = false;
        }

        QuizFeedbackText.Text = $"{(result.IsCorrect ? "Correct!" : "Not quite.")} {result.Explanation}";
        QuizFeedbackText.Foreground = new SolidColorBrush(result.IsCorrect ? Color.FromRgb(134, 239, 172) : Color.FromRgb(253, 186, 116));
        QuizScoreText.Text = $"Score: {result.CorrectAnswers} / {result.QuestionsAnswered}";
        NextQuizQuestionButton.Content = result.QuestionsAnswered == _quiz.TotalQuestions ? "Finish quiz" : "Next question";
        NextQuizQuestionButton.IsEnabled = true;
    }

    private void NextQuizQuestionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_quiz.MoveNext())
        {
            _quizAnswered = false;
            QuizFeedbackText.Text = string.Empty;
            RenderCurrentQuestion();
            QuizScoreText.Text = $"Score: {_quiz.CorrectAnswers} / {_quiz.QuestionsAnswered}";
            return;
        }

        var total = _quiz.TotalQuestions;
        var summary = _quiz.CorrectAnswers >= 10
            ? "Great job! You're a cybersecurity pro!"
            : _quiz.CorrectAnswers >= 7
                ? "Nice work. Keep practising these safe habits."
                : "Keep learning to stay safe online!";
        QuizQuestionText.Text = $"Quiz complete — {_quiz.CorrectAnswers} / {total}. {summary}";
        QuizProgressText.Text = "You completed all 12 questions.";
        QuizOptionsPanel.Children.Clear();
        QuizFeedbackText.Text = "Start the quiz again any time to practise.";
        QuizScoreText.Text = $"Final score: {_quiz.CorrectAnswers} / {total}";
        NextQuizQuestionButton.IsEnabled = false;
        SubmitQuizAnswerButton.IsEnabled = false;
        LogAction($"Cybersecurity quiz completed: {_quiz.CorrectAnswers}/{total} correct.");
        AddBotMessage($"Quiz complete: {_quiz.CorrectAnswers} out of {total}. {summary}");
    }

    private void LogAction(string description)
    {
        _activity.Insert(0, $"{DateTime.Now:yyyy-MM-dd HH:mm} — {description}");
        while (_activity.Count > 10)
        {
            _activity.RemoveAt(_activity.Count - 1);
        }
    }

    private string FormatActivityLog()
    {
        if (_activity.Count == 0)
        {
            return "There are no recorded actions yet. Add a task or start the quiz to begin your activity log.";
        }

        return "Here's a summary of recent actions:\n" + string.Join("\n", _activity.Select((entry, index) => $"{index + 1}. {entry}"));
    }

    private static bool TryExtractTask(string input, out string title, out DateTime? reminderAt)
    {
        title = string.Empty;
        reminderAt = ParseReminderDate(input);
        var match = Regex.Match(input,
            @"^(?:(?:please\s+)?(?:can\s+you\s+)?(?:add|create|make)\s+(?:a\s+)?(?:cybersecurity\s+)?(?:task|reminder)\s*(?:to\s+)?|(?:please\s+)?(?:can\s+you\s+)?set\s+(?:(?:up\s+)?(?:a\s+)?(?:cybersecurity\s+)?(?:task|reminder))\s*(?:to\s+)?|(?:please\s+)?(?:can\s+you\s+)?remind\s+me\s+(?:to|about)\s+|(?:please\s+)?remember\s+to\s+)(?<title>.+)$",
            RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return false;
        }

        title = Regex.Replace(match.Groups["title"].Value,
            @"\s+(?:(?:tomorrow)|(?:in\s+\d+\s+(?:days?|weeks?))|(?:on\s+\d{4}-\d{1,2}-\d{1,2}))\s*[.!?]*$",
            string.Empty, RegexOptions.IgnoreCase).Trim().TrimEnd('.', '!', '?');
        return title.Length > 0;
    }

    private static DateTime? ParseReminderDate(string input)
    {
        if (Regex.IsMatch(input, @"\btomorrow\b", RegexOptions.IgnoreCase))
        {
            return DateTime.Today.AddDays(1);
        }

        var interval = Regex.Match(input, @"\bin\s+(?<amount>\d+)\s+(?<unit>days?|weeks?)\b", RegexOptions.IgnoreCase);
        if (interval.Success && int.TryParse(interval.Groups["amount"].Value, out var amount) && amount is > 0 and <= 3650)
        {
            var days = interval.Groups["unit"].Value.StartsWith("week", StringComparison.OrdinalIgnoreCase) ? amount * 7 : amount;
            return DateTime.Today.AddDays(days);
        }

        var dateMatch = Regex.Match(input, @"\bon\s+(?<date>\d{4}-\d{1,2}-\d{1,2})\b", RegexOptions.IgnoreCase);
        if (dateMatch.Success && DateTime.TryParse(dateMatch.Groups["date"].Value, out var parsedDate))
        {
            return parsedDate.Date;
        }

        return null;
    }

    private static bool ContainsAny(string input, params string[] phrases) =>
        phrases.Any(phrase => input.Contains(phrase, StringComparison.OrdinalIgnoreCase));

    private void AddUserMessage(string message)
    {
        _messages.Add(new ChatMessage("You", message, true));
        ChatListBox.ScrollIntoView(_messages[^1]);
    }

    private void AddBotMessage(string message)
    {
        _messages.Add(new ChatMessage("Cyber Shield", message, false));
        ChatListBox.ScrollIntoView(_messages[^1]);
    }

    private static void PlayGreeting()
    {
        try
        {
            var assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "greeting.wav");
            if (File.Exists(assetPath))
            {
                var player = new MediaPlayer();
                player.Open(new Uri(assetPath));
                player.Play();
            }
        }
        catch
        {
            // Audio is optional; the application remains usable if playback is unavailable.
        }
    }

    private sealed record ChatMessage(string Sender, string Content, bool IsFromUser);
}
