# RaceDay and Cybersecurity Chatbot — Programming 2B Portfolio

**Institution:** Rosebank College  
**Course:** Programming 2B (PROG6212W)  
**Student:** Litha Mbawu  
**Lecturer:** Andiswa Phewa

This repository contains two related coursework deliverables: the **RaceDay Part 1 planning and database design** and the **Cybersecurity Awareness Chatbot desktop application**. This root README is the single project guide for both; the design artifacts and application source remain organized under `docs/`.

- **GitHub repository:** [LithaMbawu/Part-1](https://github.com/LithaMbawu/Part-1)
- **Project presentation:** [Watch the latest presentation on YouTube] https://youtu.be/UfLJWUsmxV0?si=ZZnDgD2nQ4lxipJZ

## Contents

- [RaceDay — Part 1 planning](#raceday--part-1-planning)
- [Cybersecurity chatbot](#cybersecurity-chatbot)
- [Repository layout](#repository-layout)
- [Submission and release checklist](#submission-and-release-checklist)

---

## RaceDay — Part 1 planning

Part 1 plans the RaceDay system before API implementation. Its deliverables are an Entity Relationship Diagram (ERD), a REST API endpoint plan, and a SQL Server database script.

### Design artifacts

- [ERD image](docs/raceday-erd.png) — six entities, keys, relationships, and cardinalities.
- [ERD Mermaid source](docs/raceday-erd.mmd) — editable source for the diagram.
- [API endpoint plan](docs/api-endpoint-plan.md) — planned routes, methods, roles, request bodies, and responses.
- [SQL Server schema and seed data](docs/raceday-schema.sql) — database, tables, constraints, and sample records.

### Database design

The schema includes `Users`, `Events`, `Categories`, `EventCategories`, `Enrolments`, and `Results`. `EventCategories` models the many-to-many relationship between events and categories; each enrolment selects an event-category pair; and a result belongs to at most one enrolment.

User roles are constrained to Admin, Organiser, and Participant. Passwords are represented by a `PasswordHash` field; the included seed hashes are placeholders for design/testing and must not be used as production credentials. Part 2 should use hashes produced by its selected password-hashing library.

### Run and verify the SQL script

1. Install or connect to a SQL Server instance and open `docs/raceday-schema.sql` in SQL Server Management Studio (SSMS).
2. Execute the complete script using an account permitted to create the `RaceDay` database and its tables.
3. Confirm the script completes without errors and select the `RaceDay` database.
4. Check the tables and constraints in Object Explorer. The script seeds the following expected row counts:

| Table | Expected rows |
| --- | ---: |
| Users | 5 |
| Events | 3 |
| Categories | 4 |
| EventCategories | 7 |
| Enrolments | 4 |
| Results | 2 |

Use the following query to verify the counts:

```sql
SELECT t.name AS TableName, SUM(p.rows) AS RowCount
FROM sys.tables AS t
JOIN sys.partitions AS p
  ON p.object_id = t.object_id AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name;
```

### Planned next stages

- **Part 2:** implement the REST API against the approved schema and endpoint plan, including authentication, role-based authorization, and tests.
- **Part 3:** build the MVC application that consumes the API, then add the planned Azure Blob Storage and Docker components.

### Assessment feedback for Part 2

> You have produced a very good submission that demonstrates a solid understanding of ASP.NET Core Web API development. Your project includes most of the required functionality, and your code is generally well organised. Your authentication, database integration and API endpoints show good progress. There are a few areas that could be improved, particularly in strengthening your unit tests and ensuring that all endpoints are properly documented and tested. Overall, this is a strong submission. Well done.

---

## Cybersecurity chatbot

The Cybersecurity Awareness Chatbot provides desktop applications for cybersecurity awareness: a Windows WPF client and a cross-platform Avalonia client for macOS, Windows, and Linux. Both share the C# core for topic-alias recognition, context-aware follow-ups, phrase-based sentiment wording, randomized tips, local personalization, flexible task/reminder parsing, a 12-question quiz, and paged activity history. The original WPF GUI has an optional bundled voice greeting; the ASCII artwork is included as a project asset, but is not currently rendered in the chat window.

### Features

- Guidance for passwords, passphrases, scams/fraud, privacy, phishing/smishing, malware/viruses, multi-factor authentication, social engineering, identity protection, backups, Wi-Fi, and general online safety.
- Multiple responses and randomized tips to make repeated interactions less repetitive.
- Follow-ups such as “tell me more” and “another tip” that continue the latest supported topic.
- Simple phrase-based sentiment detection for worried, frustrated, and curious messages.
- Local conversation memory for a name, favorite topic, most recent topic, and sentiment. Memory is saved under the current user's local application data directory and reused on the next launch. It is stored locally; remove the `CybersecurityChatbot/memory.json` file from that directory to clear it.
- Desktop interfaces with aligned chat bubbles. The Windows WPF interface also has optional playback of the supplied `docs/greeting.wav` welcome intro. The intro plays at launch and when enabled; it does not repeat after every chat message.
- A task assistant that stores task titles, descriptions, completion state, and optional reminder dates in MySQL. Add, list/read, complete/reopen, delete, update reminders, and clear reminders from the task UI. Changes reload from the database so the list reflects persisted state.
- A 12-question cybersecurity quiz with multiple-choice and true/false questions, immediate explanations, scoring, and a final result.
- Rule-based NLP command detection supports varied task/reminder phrasings and relative dates such as “tomorrow”, “in three days”, “next week”, and “in two months”; quiz and activity commands are also recognized.
- A categorized, timestamped activity log starts with the latest 10 entries, supports **Show 10 more**, and retains at most 200 actions per session. Ask “show activity log”, “show more activity”, or “show all activity” in chat.

The chatbot uses deterministic phrase and topic-alias rules with a selection of response variations; it is not an open-ended AI assistant. Sentiment detection is rule-based and may not recognize every way someone expresses a feeling. Stored reminders are not operating-system notifications: the app does not run as a background reminder service.

### Requirements

- .NET 10 SDK. The Avalonia desktop app runs on macOS, Windows, and Linux; the WPF app requires Windows 10 or later.
- A compatible Visual Studio version is needed only to build/run the Windows WPF GUI.
- The cross-platform desktop app stores tasks locally in SQLite by default. MySQL Server 8.0 or compatible is optional there; the Windows WPF client uses MySQL for task storage.
- SQL Server Management Studio and access to SQL Server only if running the RaceDay database script above.

### Configure MySQL (optional for the cross-platform app)

1. Start MySQL and create the application database by running `docs/cybersecurity-chatbot-mysql.sql` in a MySQL client.
2. Set the `CYBERSECURITY_CHATBOT_MYSQL` environment variable to a MySQL connection string for that database. Example format: `Server=localhost;Port=3306;Database=cybersecurity_chatbot;User ID=YOUR_USER;Password=YOUR_PASSWORD;`.
3. Keep credentials out of source control. Set the variable in the environment used to launch Visual Studio/the application, then restart the application so it can read the updated value.
4. On a successful MySQL connection, the application creates the `CybersecurityTasks` table if needed and loads saved tasks. The cross-platform app continues to support tasks using local SQLite if MySQL is not configured or the login fails. The Windows WPF client requires MySQL for task storage.

The selected task store keeps reminder dates at the start of the selected date. These are stored reminders, not background notifications.

### Run the chatbot on macOS, Windows, or Linux

Run the cross-platform Avalonia desktop app from the repository root:

```sh
dotnet run --project docs/CybersecurityChatbot.Desktop/CybersecurityChatbot.Desktop.csproj
```

In VS Code, choose **Terminal → Run Task → Run Mac desktop app (Avalonia)**. The default **Build** task now builds the Mac-compatible project instead of the Windows WPF window.

Task CRUD works without MySQL: the Mac/cross-platform app uses a local SQLite database at `~/Library/Application Support/CybersecurityChatbot/tasks.db` on macOS, with no account or password required. MySQL remains optional: enter its host, port, database, username, and password in the Tasks tab and select **Connect** to switch storage. The password is masked and not saved. See [the cross-platform app notes](docs/CybersecurityChatbot.Desktop/README.md).

### Run the Windows WPF chatbot

1. Clone or download this repository on Windows.
2. Open `docs/CybersecurityChatbot.slnx` in Visual Studio.
3. Set `CybersecurityChatbot.Gui` as the startup project.
4. Restore/build the solution and press **F5**.
5. Type a question into the chat box and press **Enter** or select **Send**. Use the voice toggle to enable or disable the greeting playback.

The core and test projects target .NET 8; the WPF application targets .NET 10 for Windows and references the core project. The supplied intro WAV is copied to the GUI output directory at build time. Voice playback is optional and the application continues if playback is unavailable.

### Run the automated tests

From the repository root, run `dotnet test docs/CybersecurityChatbot.slnx`. The core tests cover topic aliases, follow-up context, sentiment phrasing, memory restoration, flexible task/reminder parsing, activity paging/retention, and quiz behavior. Tests do not require a live MySQL server; verify database integration separately against the configured database.

### Example conversation

```text
You: My name is Litha
Cyber Shield: Nice to meet you, Litha. I’ll remember your name and keep our conversation personal and helpful.

You: I am interested in phishing
Cyber Shield: Great! I’ll remember that you’re interested in phishing. It is a crucial part of staying safe online.

You: I feel worried about phishing
Cyber Shield: It’s completely understandable to feel that way. [One of the phishing safety responses]

You: tell me more
Cyber Shield: Absolutely — let’s keep talking about phishing. [A further phishing tip]
```

Exact wording and advice can vary because responses are selected from multiple options.

### Project structure

```text
.
├── README.md
├── .github/workflows/                 # Repository documentation validation
└── docs/
    ├── CybersecurityChatbot.slnx
    ├── CybersecurityChatbot.Core/     # Response, memory, NLP parsing, quiz, and activity logic
    ├── CybersecurityChatbot.Core.Tests/ # Core behavior and memory tests
    ├── CybersecurityChatbot.Gui/      # Windows WPF interface
    ├── CybersecurityChatbot.Desktop/  # Cross-platform Avalonia interface
    ├── cybersecurity-chatbot-mysql.sql # MySQL database setup
    ├── assets/                        # Voice and ASCII assets
    ├── api-endpoint-plan.md
    ├── cybersecurity-chatbot-presentation-guide.md # Demo script and code walkthrough
    ├── raceday-erd.mmd
    ├── raceday-erd.png
    └── raceday-schema.sql
```

---

## Submission and release checklist

- Submit the repository link through ARC: [LithaMbawu/Part-1](https://github.com/LithaMbawu/Part-1).
- Submit the presentation link through ARC or use the YouTube link above. The video is listed as unlisted; confirm the assessor can access it.
- Use the [presentation guide](docs/cybersecurity-chatbot-presentation-guide.md) to record a clear feature and code walkthrough. Update the YouTube URL only after uploading the final video.
- Attach the available evidence: [CI workflow screenshot](docs/ci-workflow-screenshot.png) and [SSMS execution screenshot](docs/ssms-execution-screenshot.png). Confirm each image clearly shows the relevant successful run before submission.
- Keep commits descriptive and tied to real project changes; do not manufacture commits to meet a count.
- Three GitHub Releases with annotated version notes are published below. Keep release notes accurate if these milestones change; a tag alone is not a GitHub Release.

### Version notes

- **[v0.1.0 — RaceDay planning baseline](https://github.com/LithaMbawu/Part-1/releases/tag/v0.1.0):** initial ERD, API endpoint plan, SQL Server schema, and planning documentation.
- **[v0.2.0 — RaceDay evidence update](https://github.com/LithaMbawu/Part-1/releases/tag/v0.2.0):** adds the CI workflow evidence while retaining the ERD, endpoint plan, and database script.
- **[v1.0.0 — Combined coursework portfolio](https://github.com/LithaMbawu/Part-1/releases/tag/v1.0.0):** brings the RaceDay plan and Windows cybersecurity chatbot together, with persistent conversation memory, automated core tests, and a refreshed chat interface.

These notes describe the corresponding repository milestones and link to the published GitHub Releases.
