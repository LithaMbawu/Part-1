using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private readonly ObservableCollection<string> _messages = new();

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
        VoiceToggleButton.Content = VoiceToggleButton.IsChecked == true ? "🔊 Voice on" : "🔇 Voice off";
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

        if (VoiceToggleButton.IsChecked == true)
        {
            PlayGreeting();
        }

        UserInputTextBox.Clear();
        UserInputTextBox.Focus();
    }

    private void AddUserMessage(string message)
    {
        _messages.Add($"You: {message}");
    }

    private void AddBotMessage(string message)
    {
        _messages.Add($"Cyber Shield: {message}");
    }

    private static void PlayGreeting()
    {
        try
        {
            var assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "voice-greeting.wav");
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

        try
        {
            Process.Start("say", "Cyber security assistant ready");
        }
        catch
        {
            // Voice is optional and should fail gracefully.
        }
    }
}