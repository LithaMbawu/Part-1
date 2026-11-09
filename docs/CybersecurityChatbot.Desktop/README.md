# Cyber Shield Assistant — cross-platform desktop app

This Avalonia desktop client runs on macOS, Windows, and Linux. It shares the chatbot core and MySQL task repository with the original Windows WPF client.

## Run on macOS

From the repository root, run:

```sh
dotnet run --project docs/CybersecurityChatbot.Desktop/CybersecurityChatbot.Desktop.csproj
```

In VS Code, use **Terminal → Run Task → Run Mac desktop app (Avalonia)**. **Tasks: Run Build Task** builds this Mac-compatible project by default.

The first run restores Avalonia and MySQL dependencies. .NET 10 SDK is required. The app can be used without MySQL for chat, quiz, and the in-memory activity log. To enable persisted tasks, configure `CYBERSECURITY_CHATBOT_MYSQL` with a MySQL connection string before starting the app. See the repository README for database setup details.

The Tasks tab can add tasks with descriptions and an optional calendar reminder, list saved tasks, mark them complete/reopen them, update or clear their reminders, and delete them. The reminder date is stored in MySQL; it does not generate an OS notification or run a background service. Chat also understands common task phrasing such as “Remind me to update my password tomorrow” and “I need to back up my phone next week.”

The activity tab shows 10 recent categorized, timestamped actions initially. Use **Show 10 more**, or ask the chat to “show more activity” or “show all activity.” Up to 200 actions are retained for the current session.

The WPF project remains available for Windows users; this project is the macOS-compatible UI.
