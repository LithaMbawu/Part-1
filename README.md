# RaceDay and Cybersecurity Chatbot — Programming 2B Portfolio

**Institution:** Rosebank College  
**Course:** Programming 2B (PROG6212W)  
**Student:** Litha Mbawu  
**Lecturer:** Andiswa Phewa

This repository contains two related coursework deliverables: the **RaceDay Part 1 planning and database design** and the **Cybersecurity Awareness Chatbot desktop application**. This root README is the single project guide for both; the design artifacts and application source remain organized under `docs/`.

- **GitHub repository:** [LithaMbawu/Part-1](https://github.com/LithaMbawu/Part-1)
- **Project presentation:** [Watch on YouTube](https://youtu.be/0OgLvuN1I5w)

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

---

## Cybersecurity chatbot

The Cybersecurity Awareness Chatbot is a C# WPF desktop application for cybersecurity awareness. Its reusable core provides keyword-based guidance, follow-up responses, simple sentiment-aware phrasing, randomized tips, and in-session personalization. The GUI has a dark chat layout, cyber-themed ASCII branding, and an optional bundled voice greeting.

### Features

- Guidance for password safety, scams, privacy, phishing, malware, and general online safety.
- Multiple responses and randomized tips to make repeated interactions less repetitive.
- Follow-ups such as “tell me more” and “another tip” that continue the latest supported topic.
- Simple phrase-based sentiment detection for worried, frustrated, and curious messages.
- Local conversation memory for a name, favorite topic, most recent topic, and sentiment. Memory is saved under the current user's local application data directory and reused on the next launch. It is stored locally; remove the `CybersecurityChatbot/memory.json` file from that directory to clear it.
- A WPF interface with aligned chat bubbles and optional playback of the bundled `voice-greeting.wav` welcome greeting. The greeting plays at launch and when enabled; it does not repeat after every chat message.

The chatbot is keyword/phrase-driven, not an open-ended AI assistant. Sentiment detection is rule-based and may not recognize every way someone expresses a feeling.

### Requirements

- Windows 10 or later (the GUI uses WPF).
- .NET 10 SDK and a compatible Visual Studio version to build and run the GUI.
- SQL Server Management Studio and access to SQL Server only if running the RaceDay database script above.

### Run the chatbot

1. Clone or download this repository on Windows.
2. Open `docs/CybersecurityChatbot.slnx` in Visual Studio.
3. Set `CybersecurityChatbot.Gui` as the startup project.
4. Restore/build the solution and press **F5**.
5. Type a question into the chat box and press **Enter** or select **Send**. Use the voice toggle to enable or disable the greeting playback.

The core and test projects target .NET 8; the WPF application targets .NET 10 for Windows and references the core project. The bundled WAV is copied to the GUI output directory at build time. Voice playback is optional and the application continues if playback is unavailable.

### Run the automated tests

From the repository root, run `dotnet test docs/CybersecurityChatbot.slnx`. The core tests cover supported-topic recognition, follow-up context, sentiment phrasing, and restoring memory in a new session.

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
    ├── CybersecurityChatbot.Core/     # Response logic and persistent memory
    ├── CybersecurityChatbot.Core.Tests/ # Core behavior and memory tests
    ├── CybersecurityChatbot.Gui/      # Windows WPF interface
    ├── assets/                        # Voice and ASCII assets
    ├── api-endpoint-plan.md
    ├── raceday-erd.mmd
    ├── raceday-erd.png
    └── raceday-schema.sql
```

---

## Submission and release checklist

- Submit the repository link through ARC: [https://github.com/LithaMbawu/Part-1](https://github.com/LithaMbawu/Part-1).
- Submit the presentation link through ARC or use the YouTube link above.
- Attach the available evidence: [CI workflow screenshot](docs/ci-workflow-screenshot.png) and [SSMS execution screenshot](docs/ssms-execution-screenshot.png). Confirm each image clearly shows the relevant successful run before submission.
- Keep commits descriptive and tied to real project changes; do not manufacture commits to meet a count.
- The rubric requests at least three tagged releases with version notes. Create those tags/releases in GitHub as actual milestones are completed, with accurate notes describing what each version contains. A tag is not the same as a GitHub release.

### Version notes

- **[v0.1.0 — RaceDay planning baseline](https://github.com/LithaMbawu/Part-1/releases/tag/v0.1.0):** initial ERD, API endpoint plan, SQL Server schema, and planning documentation.
- **[v0.2.0 — RaceDay evidence update](https://github.com/LithaMbawu/Part-1/releases/tag/v0.2.0):** adds the CI workflow evidence while retaining the ERD, endpoint plan, and database script.
- **[v1.0.0 — Combined coursework portfolio](https://github.com/LithaMbawu/Part-1/releases/tag/v1.0.0):** brings the RaceDay plan and Windows cybersecurity chatbot together, with persistent conversation memory, automated core tests, and a refreshed chat interface.

These notes describe the corresponding repository milestones. Publish the matching annotated tags as GitHub Releases so they appear on the repository's Releases page.
