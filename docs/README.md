RaceDay Part 1 Planning

Institution: Rosebank College
Course: Programming 2B
Course Code: PROG6212W
Name: Litha
Surname: Mbawu
Lecturer: Andiswa Phewa

⸻

Overview

This folder contains the design artifacts required before API implementation begins. Part 1 focuses on planning the RaceDay system and establishing a clear database structure and API contract that will guide development during Part 2 and Part 3.

Completing the planning stage before implementation helps prevent unnecessary changes, inconsistencies, and rework during later stages of the project.

Contents

* raceday-erd.png — ERD export showing six entities, keys, cardinalities, and the many-to-many category relationship.
* raceday-erd.mmd — Mermaid source for the ERD, retained for future updates.
* api-endpoint-plan.md — Planned REST API contract for authentication, profiles, events, categories, enrolments, and results.
* raceday-schema.sql — SQL Server schema, constraints, and realistic seed data.

⸻

Purpose of Part 1

Part 1 focuses on planning the RaceDay system before any API implementation begins.

The purpose of this stage is to establish a clear database structure and API contract that can be followed during Part 2 and Part 3. Completing the planning first helps prevent unnecessary changes and rework during later development.

The three main deliverables are:

* Entity Relationship Diagram (ERD)
* REST API Endpoint Plan
* SQL Server Database Script

⸻

Entity Relationship Diagram

The RaceDay ERD represents the complete database design for the system.

The ERD contains six entities and identifies:

* Primary keys
* Foreign keys
* Entity attributes
* One-to-many relationships
* Many-to-many relationships
* Cardinality between related entities

The ERD is exported as a PNG for submission, while the Mermaid source is retained so that the design can be updated if required during development.

ERD Files

* raceday-erd.png
* raceday-erd.mmd

The SQL database script is designed to match the ERD so that the database implementation remains consistent with the planned data model.


⸻

API Endpoint Plan

The API endpoint plan defines the RESTful API contract that will be implemented during Part 2.

The plan covers the required functionality:

* Authentication
* User registration
* User login
* User profiles
* Events
* Categories
* Event enrolments
* Results

Each endpoint specifies the relevant:

* HTTP method
* Route
* Description
* Required role
* Request body, where applicable
* Expected response

The endpoint plan is completed before API implementation so that the Part 2 API can be developed against an approved design.

API File

api-endpoint-plan.md

Any deliberate differences between the planned endpoints and the final implementation should be documented and explained.

⸻

SQL Database Script

The SQL Server database script creates the database structure represented in the ERD.

The script includes:

* Database and table creation
* Primary keys
* Foreign keys
* NOT NULL constraints
* UNIQUE constraints
* DEFAULT values where required
* Sample data

The script also contains realistic seed data required for testing the system.

SQL File

raceday-schema.sql

### SSMS setup and verification

1. Open `raceday-schema.sql` in SQL Server Management Studio connected to a SQL Server instance where the account can create databases.
2. Execute the complete script from the beginning. The script creates or reuses the `RaceDay` database, recreates the six tables in dependency order, and inserts the seed data.
3. Confirm that the Messages window reports successful completion with no errors.
4. Run the following verification query in the `RaceDay` database:

```sql
SELECT t.name AS TableName, SUM(p.rows) AS RowCount
FROM sys.tables AS t
JOIN sys.partitions AS p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name;
```

The expected row counts are `Users = 5`, `Events = 3`, `Categories = 4`, `EventCategories = 7`, `Enrolments = 4`, and `Results = 2`. Inspect the Keys and Constraints folders in SSMS to confirm the named primary keys, foreign keys, unique constraints, defaults, and check constraints.

The repository does not claim that an SSMS execution, screenshot, or video exists until that evidence is added below.

⸻

Design Decisions

User Roles

Users.Role is implemented as a constrained value with three possible roles:

* Admin
* Organiser
* Participant

A separate role entity was not created because it is not required by the brief. Using a constrained role value keeps the role-based authorization model explicit while avoiding an unnecessary role join.

This role structure will also support the role-based authorization requirements that will be implemented in Part 2.

⸻

Password Security

Passwords are represented by a PasswordHash field.

Plaintext passwords are never stored in the database. The database therefore stores the password hash rather than the original password entered by the user.

The seed data currently uses placeholder BCrypt-style password hashes for documentation purposes. During Part 2, these values must be replaced with hashes generated using the password-hashing implementation selected for the API before the seeded accounts are used in a real environment.

⸻

Event and Category Relationships

An event can offer several categories, and a category can be offered by several events.

This creates a many-to-many relationship between Events and Categories.

The relationship is implemented using the EventCategories entity.

Conceptually:

Event
  │
  │ 1
  │
  │ *
EventCategories
  │
  │ *
  │
  │ 1
Category

This allows the same category to be associated with multiple events while allowing each event to offer multiple categories.

⸻

Enrolments

An enrolment selects one event category.

This means an enrolment is associated with a specific category offered by a specific event through EventCategories.

This design prevents an enrolment from selecting a category that is not actually available for the chosen event.

The relationship can be represented as:

Event → EventCategories → Category
                 │
                 │
                 ▼
             Enrolment

⸻

Results

A result belongs to at most one enrolment.

This is implemented using a foreign key from the Results entity to Enrolments, with a UNIQUE constraint on that foreign key.

This ensures that an enrolment cannot have multiple results recorded against it.

Conceptually:

Enrolment 1 ───────── 0..1 Result

Therefore, an enrolment may have no result yet, or it may have exactly one result once the participant has completed the race.

⸻

Database and ERD Consistency

The SQL script and ERD use the same six entities and relationships.

The database design follows the structure shown in the ERD, including:

* The six entities
* Primary keys
* Foreign keys
* Cardinalities
* The many-to-many Event–Category relationship through EventCategories
* The relationship between Enrolments and Results

Any future changes to the database structure should first be reflected in the planning documents so that the ERD, SQL script, and API design remain consistent.

⸻

Seed Data

The SQL script contains realistic sample data to allow the database to be tested before the API is implemented.

The seed data includes:

* Admin, Organiser, and Participant users as required by the system design
* At least two Organisers
* At least two Participants
* Three Events
* Categories for each event
* Sample EventCategories relationships
* Sample participant enrolments
* Sample results where applicable

The seed data is intended for development and testing purposes.

⸻

Password Hash Note

The seeded password values are placeholder BCrypt-style hashes for documentation and database demonstration.

These should not be treated as production credentials.

Part 2 must generate appropriate password hashes using the API’s selected password-hashing implementation before these accounts are used in a real environment.

⸻

Relationship Between Part 1, Part 2 and Part 3

Part 1 provides the foundation for the remaining stages of the RaceDay project.

Part 1 – Planning and Database

ERD
 │
 ├── Defines entities
 ├── Defines attributes
 └── Defines relationships
 │
 ▼
SQL Database
 │
 ├── Creates tables
 ├── Creates keys and constraints
 └── Inserts seed data
 │
 ▼
API Endpoint Plan
 └── Defines the REST API contract

⸻

Part 2 – RESTful API

The approved database and API plan will be used to build the C# RESTful API.

The API will:

* Connect to the SQL Server database
* Implement the planned endpoints
* Apply role-based authorization
* Handle authentication
* Include unit tests
* Use GitHub CI/CD

⸻

Part 3 – MVC Application

The MVC web application will consume the API developed in Part 2.

Part 3 will also introduce:

* Azure Blob Storage
* Docker containerisation
* Version Control

All Part 1 planning documents are stored in this folder and should be committed to the GitHub repository before API implementation begins.

Keeping the original planning documents in the repository provides evidence that the system was planned before the API code was written.

The Mermaid ERD source is also retained to make future design updates easier while preserving the submitted PNG version.

## Submission evidence

Add the actual links before submission:

* CI workflow run screenshot: 
* Professional explanation video: `TODO - add video link`
* Live SSMS execution screenshot: `TODO - add screenshot link`

These items cannot be generated honestly from the source files alone. The commit-history requirement also requires at least 20 genuine, meaningful commits; the current repository history must be extended through normal development work rather than artificially duplicated commits.

⸻

Deviations from the Plan

No deliberate deviations have been made between the ERD and SQL database design.

The six entities and their relationships represented in the ERD are implemented in the SQL schema.

The API endpoint plan is intended to be followed closely during Part 2. If implementation changes are required, those changes should be documented and explained so that the planning documents remain traceable to the final implementation.

⸻