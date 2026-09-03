/* RaceDay Part 1 schema and seed data. SQL Server / SSMS. */
IF DB_ID(N'RaceDay') IS NULL
    CREATE DATABASE RaceDay;
GO
USE RaceDay;
GO

DROP TABLE IF EXISTS dbo.Results;
DROP TABLE IF EXISTS dbo.Enrolments;
DROP TABLE IF EXISTS dbo.EventCategories;
DROP TABLE IF EXISTS dbo.Categories;
DROP TABLE IF EXISTS dbo.Events;
DROP TABLE IF EXISTS dbo.Users;
GO

CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    FirstName NVARCHAR(60) NOT NULL,
    LastName NVARCHAR(60) NOT NULL,
    Email NVARCHAR(320) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Organiser', N'Participant')),
    Phone NVARCHAR(30) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.Events (
    EventId INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    Description NVARCHAR(1000) NULL,
    EventDate DATETIME2(0) NOT NULL,
    Venue NVARCHAR(200) NOT NULL,
    Capacity INT NOT NULL CONSTRAINT CK_Events_Capacity CHECK (Capacity > 0),
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Events_Status DEFAULT N'Open' CONSTRAINT CK_Events_Status CHECK (Status IN (N'Draft', N'Open', N'Closed', N'Cancelled', N'Completed')),
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserId) REFERENCES dbo.Users(UserId)
);

CREATE TABLE dbo.Categories (
    CategoryId INT IDENTITY(1,1) CONSTRAINT PK_Categories PRIMARY KEY,
    Name NVARCHAR(80) NOT NULL CONSTRAINT UQ_Categories_Name UNIQUE,
    Description NVARCHAR(500) NULL
);

CREATE TABLE dbo.EventCategories (
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL CONSTRAINT CK_EventCategories_EntryFee CHECK (EntryFee >= 0),
    CONSTRAINT PK_EventCategories PRIMARY KEY (EventId, CategoryId),
    CONSTRAINT FK_EventCategories_Event FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId),
    CONSTRAINT FK_EventCategories_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId)
);

CREATE TABLE dbo.Enrolments (
    EnrolmentId INT IDENTITY(1,1) CONSTRAINT PK_Enrolments PRIMARY KEY,
    EventId INT NOT NULL,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolledAt DATETIME2(0) NOT NULL CONSTRAINT DF_Enrolments_EnrolledAt DEFAULT SYSUTCDATETIME(),
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT N'Pending' CONSTRAINT CK_Enrolments_Status CHECK (Status IN (N'Pending', N'Confirmed', N'Withdrawn')),
    BibNumber INT NULL CONSTRAINT UQ_Enrolments_BibNumber UNIQUE,
    CONSTRAINT UQ_Enrolments_Participant_Event UNIQUE (EventId, ParticipantId),
    CONSTRAINT FK_Enrolments_Event_Category FOREIGN KEY (EventId, CategoryId) REFERENCES dbo.EventCategories(EventId, CategoryId),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantId) REFERENCES dbo.Users(UserId)
);

CREATE TABLE dbo.Results (
    ResultId INT IDENTITY(1,1) CONSTRAINT PK_Results PRIMARY KEY,
    EnrolmentId INT NOT NULL CONSTRAINT UQ_Results_Enrolment UNIQUE,
    Position INT NULL CONSTRAINT CK_Results_Position CHECK (Position IS NULL OR Position > 0),
    FinishTimeSeconds INT NULL CONSTRAINT CK_Results_FinishTime CHECK (FinishTimeSeconds IS NULL OR FinishTimeSeconds > 0),
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Results_Status DEFAULT N'Published' CONSTRAINT CK_Results_Status CHECK (Status IN (N'Pending', N'Published', N'Disqualified')),
    RecordedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Results_RecordedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (EnrolmentId) REFERENCES dbo.Enrolments(EnrolmentId)
);
GO

INSERT dbo.Users (FirstName, LastName, Email, PasswordHash, Role, Phone) VALUES
(N'Leah', N'Naidoo', N'leah.organiser@raceday.test', N'$2b$12$replace-with-real-api-hash-000000000000000000000', N'Organiser', N'+27 11 555 0101'),
(N'Marko', N'Jacobs', N'marko.organiser@raceday.test', N'$2b$12$replace-with-real-api-hash-111111111111111111111', N'Organiser', N'+27 21 555 0102'),
(N'Aisha', N'Khan', N'aisha.participant@raceday.test', N'$2b$12$replace-with-real-api-hash-222222222222222222222', N'Participant', N'+27 82 555 0103'),
(N'Thabo', N'Mokoena', N'thabo.participant@raceday.test', N'$2b$12$replace-with-real-api-hash-333333333333333333333', N'Participant', N'+27 83 555 0104'),
(N'Rina', N'Patel', N'rina.admin@raceday.test', N'$2b$12$replace-with-real-api-hash-444444444444444444444', N'Admin', N'+27 84 555 0105');

INSERT dbo.Categories (Name, Description) VALUES
(N'5K Fun Run', N'An accessible five kilometre road race.'),
(N'10K Road Race', N'A timed ten kilometre road race.'),
(N'Half Marathon', N'A 21.1 kilometre endurance race.'),
(N'Trail Challenge', N'A marked off-road trail event.');

INSERT dbo.Events (OrganiserId, Name, Description, EventDate, Venue, Capacity, Status) VALUES
(1, N'Cape Sunrise Run', N'A sunrise route along the promenade.', '2026-10-10T06:30:00', N'Green Point Promenade', 500, N'Open'),
(1, N'Johannesburg Spring Classic', N'A city road race supporting local schools.', '2026-10-24T07:00:00', N'Zoo Lake Sports Grounds', 750, N'Open'),
(2, N'Drakensberg Trail Day', N'Scenic mountain trails for experienced runners.', '2026-11-07T06:00:00', N'Royal Natal Park Gate', 250, N'Open');

INSERT dbo.EventCategories (EventId, CategoryId, EntryFee) VALUES
(1, 1, 80.00), (1, 2, 130.00), (2, 1, 75.00), (2, 2, 125.00), (2, 3, 220.00), (3, 1, 100.00), (3, 4, 180.00);

INSERT dbo.Enrolments (EventId, ParticipantId, CategoryId, Status, BibNumber) VALUES
(1, 3, 2, N'Confirmed', 101),
(1, 4, 1, N'Confirmed', 102),
(2, 3, 3, N'Pending', 201),
(3, 4, 4, N'Confirmed', 301);

INSERT dbo.Results (EnrolmentId, Position, FinishTimeSeconds, Status) VALUES
(1, 2, 3120, N'Published'),
(2, 1, 1685, N'Published');
GO