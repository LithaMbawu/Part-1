# Cyber Shield Assistant — presentation guide

Use this outline to record or refresh the project presentation linked from the root README. It is a demo script and recording checklist, not a substitute for uploading a video. Show the real running application and the real build/test results; do not present unconfigured MySQL as connected.

## Suggested 6–8 minute walkthrough

### 1. Purpose and structure (about 45 seconds)

Introduce Cyber Shield Assistant as a rule-based cybersecurity awareness chatbot with a shared C# core, a macOS/Windows/Linux Avalonia client, and a Windows-only WPF client. Explain that chat, tasks, reminders, a quiz, and an activity history are grouped into separate tabs. Mention that the application is not a generative AI assistant and does not send conversations to an AI service.

Show the repository layout briefly:

- `CybersecurityChatbot.Core` contains reusable response, memory, task parsing, quiz, and activity-history logic.
- `CybersecurityChatbot.Desktop` is the Avalonia client for macOS and other desktop platforms.
- `CybersecurityChatbot.Gui` is the Windows WPF client.
- `CybersecurityChatbot.Core.Tests` contains automated behavior tests.

### 2. Chat and personalization (about 1 minute)

Try a direct question such as “How do I make my passwords safer?” Then demonstrate a variation like “Should I use a different passphrase for every account?” Explain that the response service maps common aliases to topics and chooses among multiple guidance responses. Try “I’m worried about phishing” and then “tell me more” to show phrase-based sentiment-aware wording and saved topic context. Say “Call me Litha” and “hello” to demonstrate local name personalization. State that these are explicit phrase rules, not a trained language model.

### 3. Task and reminder workflow (about 1½ minutes)

If a MySQL database is configured, add a task with its title and description, choose a reminder date, and save it. Demonstrate completing/reopening a task, changing its reminder, clearing its reminder, and deleting a task. Show the list refresh after each database write.

In chat, demonstrate several natural phrasings, for example:

- “Remind me to update my recovery email tomorrow.”
- “Could you add a task to enable MFA in three days?”
- “I need to back up my phone next week.”

Explain that the shared `TaskRequestParser` recognizes common command forms, extracts the task title, and translates relative dates into stored dates. The MySQL repository uses parameterized SQL for inserts and updates. If the database is unavailable, the interface reports that task storage is disabled instead of pretending the write succeeded. These are stored reminders; the app is not a background notification service.

If no database is available during recording, show the setup instructions and status message instead. Do not claim that a database operation succeeded without verifying it.

### 4. Quiz (about 1 minute)

Start the cybersecurity quiz, select an answer, submit it, and explain the immediate correctness feedback and explanation. Continue through a few questions and point out the score and final result. The quiz has 12 multiple-choice and true/false questions. `QuizService` tracks the current question, prevents duplicate answers, validates option indexes, and maintains the score independently of the UI.

### 5. Activity history (about 45 seconds)

Show that task, reminder, quiz, and recognized activity commands are timestamped and categorized. The activity tab starts with the latest 10 entries; select **Show 10 more** or ask “show more activity.” Ask “show activity log” in chat to see a concise summary, or “show all activity” to expand the view. Explain that the in-memory history is bounded to 200 actions for the current app session.

### 6. Implementation and quality checks (about 1 minute)

Describe how `ChatbotMemory` stores a small local JSON record using a temporary file followed by replacement, while continuing to work if optional memory storage fails. Explain that `MySqlTaskRepository` initializes the task table and performs reads and CRUD operations asynchronously.

Show the test output. The core tests cover supported topics and aliases, sentiment and follow-up context, memory restoration, flexible task/reminder parsing, activity paging/retention, and quiz behavior. Mention separately that a passing core test suite does not prove a live MySQL server is configured; demonstrate that integration only when connected to the intended database.

## Final recording checklist

- Record the actual Avalonia client on macOS, or the WPF client on Windows.
- Keep MySQL credentials out of the recording and repository.
- Show a successful build and test summary, not generated files under `bin` or `obj`.
- Verify any task/reminder write in the running UI (and database, if available).
- Keep the existing YouTube URL in the README accurate and accessible to the assessor; update the link only after uploading a new presentation.
