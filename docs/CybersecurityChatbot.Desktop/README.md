# Cyber Shield Assistant — cross-platform desktop app

This Avalonia desktop client runs on macOS, Windows, and Linux. It shares the chatbot core and MySQL task repository with the original Windows WPF client.

## Run on macOS

From the repository root, run:

```sh
dotnet run --project docs/CybersecurityChatbot.Desktop/CybersecurityChatbot.Desktop.csproj
```

The first run restores Avalonia and MySQL dependencies. .NET 10 SDK is required. The app can be used without MySQL for chat, quiz, and the in-memory activity log. To enable persisted tasks, configure `CYBERSECURITY_CHATBOT_MYSQL` with a MySQL connection string before starting the app. See the repository README for database setup details.

The WPF project remains available for Windows users; this project is the macOS-compatible UI.
