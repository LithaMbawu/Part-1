using System.Collections.ObjectModel;
using System.IO;
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
    private readonly ObservableCollection<ChatMessage> _messages = new();

    public MainWindow()
    {
        InitializeComponent();
        ChatListBox.ItemsSource = _messages;
        AddBotMessage("Welcome! I’m your cybersecurity guide. Ask me about passwords, scams, privacy, phishing, or safe online habits.");

        if (VoiceToggleButton.IsChecked == true)
        {
            PlayGreeting();
        }
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        HandleUserInput();
    }

    private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            HandleUserInput();
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

    private void HandleUserInput()
    {
        var input = UserInputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        AddUserMessage(input);
        var response = _chatbot.GetResponse(input, _memory);
        AddBotMessage(response);

        UserInputTextBox.Clear();
        UserInputTextBox.Focus();
    }

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
                return;
            }
        }
        catch
        {
            // Ignore playback issues and continue gracefully.
        }
    }

    private sealed record ChatMessage(string Sender, string Content, bool IsFromUser);
}
