/* ============================================================
   RaceDay Part 1
   SQLite Database Schema + Seed Data
   ============================================================ */

/* Enable foreign-key enforcement in SQLite */
PRAGMA foreign_keys = ON;


/* ============================================================
   1. DROP EXISTING TABLES
   ============================================================ */

DROP TABLE IF EXISTS Results;
DROP TABLE IF EXISTS Enrolments;
DROP TABLE IF EXISTS EventCategories;
DROP TABLE IF EXISTS Categories;
DROP TABLE IF EXISTS Events;
DROP TABLE IF EXISTS Users;


/* ============================================================
   2. USERS
   ============================================================ */

CREATE TABLE Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,

    FirstName TEXT NOT NULL,

    LastName TEXT NOT NULL,

    Email TEXT NOT NULL UNIQUE,

    PasswordHash TEXT NOT NULL,

    Role TEXT NOT NULL
        CHECK (Role IN (
            'Admin',
            'Organiser',
            'Participant'
        )),

    Phone TEXT,

    CreatedAt TEXT NOT NULL
        DEFAULT CURRENT_TIMESTAMP
);


/* ============================================================
   3. EVENTS
   ============================================================ */

CREATE TABLE Events (
    EventId INTEGER PRIMARY KEY AUTOINCREMENT,

    OrganiserId INTEGER NOT NULL,

    Name TEXT NOT NULL,

    Description TEXT,

    EventDate TEXT NOT NULL,

    Venue TEXT NOT NULL,

    Capacity INTEGER NOT NULL
        CHECK (Capacity > 0),

    Status TEXT NOT NULL
        DEFAULT 'Open'
        CHECK (Status IN (
            'Draft',
            'Open',
            'Closed',
            'Cancelled',
            'Completed'
        )),

    CreatedAt TEXT NOT NULL
        DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (OrganiserId)
        REFERENCES Users(UserId)
);


/* ============================================================
   4. CATEGORIES
   ============================================================ */

CREATE TABLE Categories (
    CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,

    Name TEXT NOT NULL UNIQUE,

    Description TEXT
);


/* ============================================================
   5. EVENT CATEGORIES
   ============================================================ */

CREATE TABLE EventCategories (
    EventId INTEGER NOT NULL,

    CategoryId INTEGER NOT NULL,

    EntryFee REAL NOT NULL
        CHECK (EntryFee >= 0),

    PRIMARY KEY (EventId, CategoryId),

    FOREIGN KEY (EventId)
        REFERENCES Events(EventId),

    FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId)
);


/* ============================================================
   6. ENROLMENTS
   ============================================================ */

CREATE TABLE Enrolments (
    EnrolmentId INTEGER PRIMARY KEY AUTOINCREMENT,

    EventId INTEGER NOT NULL,

    ParticipantId INTEGER NOT NULL,

    CategoryId INTEGER NOT NULL,

    EnrolledAt TEXT NOT NULL
        DEFAULT CURRENT_TIMESTAMP,

    Status TEXT NOT NULL
        DEFAULT 'Pending'
        CHECK (Status IN (
            'Pending',
            'Confirmed',
            'Withdrawn'
        )),

    BibNumber INTEGER UNIQUE,

    /* A participant can only enrol once per event */
    UNIQUE (EventId, ParticipantId),

    FOREIGN KEY (EventId, CategoryId)
        REFERENCES EventCategories(EventId, CategoryId),

    FOREIGN KEY (ParticipantId)
        REFERENCES Users(UserId)
);


/* ============================================================
   7. RESULTS
   ============================================================ */

CREATE TABLE Results (
    ResultId INTEGER PRIMARY KEY AUTOINCREMENT,

    EnrolmentId INTEGER NOT NULL UNIQUE,

    Position INTEGER
        CHECK (
            Position IS NULL
            OR Position > 0
        ),

    FinishTimeSeconds INTEGER
        CHECK (
            FinishTimeSeconds IS NULL
            OR FinishTimeSeconds > 0
        ),

    Status TEXT NOT NULL
        DEFAULT 'Published'
        CHECK (Status IN (
            'Pending',
            'Published',
            'Disqualified'
        )),

    RecordedAt TEXT NOT NULL
        DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (EnrolmentId)
        REFERENCES Enrolments(EnrolmentId)
);


/* ============================================================
   8. INSERT USERS
   ============================================================ */

INSERT INTO Users (
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Role,
    Phone
)
VALUES
(
    'Leah',
    'Naidoo',
    'leah.organiser@raceday.test',
    '$2b$12$replace-with-real-api-hash-000000000000000000000',
    'Organiser',
    '+27 11 555 0101'
),
(
    'Marko',
    'Jacobs',
    'marko.organiser@raceday.test',
    '$2b$12$replace-with-real-api-hash-111111111111111111111',
    'Organiser',
    '+27 21 555 0102'
),
(
    'Aisha',
    'Khan',
    'aisha.participant@raceday.test',
    '$2b$12$replace-with-real-api-hash-222222222222222222222',
    'Participant',
    '+27 82 555 0103'
),
(
    'Thabo',
    'Mokoena',
    'thabo.participant@raceday.test',
    '$2b$12$replace-with-real-api-hash-333333333333333333333',
    'Participant',
    '+27 83 555 0104'
),
(
    'Rina',
    'Patel',
    'rina.admin@raceday.test',
    '$2b$12$replace-with-real-api-hash-444444444444444444444',
    'Admin',
    '+27 84 555 0105'
);


/* ============================================================
   9. INSERT CATEGORIES
   ============================================================ */

INSERT INTO Categories (
    Name,
    Description
)
VALUES
(
    '5K Fun Run',
    'An accessible five kilometre road race.'
),
(
    '10K Road Race',
    'A timed ten kilometre road race.'
),
(
    'Half Marathon',
    'A 21.1 kilometre endurance race.'
),
(
    'Trail Challenge',
    'A marked off-road trail event.'
);


/* ============================================================
   10. INSERT EVENTS
   ============================================================ */

INSERT INTO Events (
    OrganiserId,
    Name,
    Description,
    EventDate,
    Venue,
    Capacity,
    Status
)
VALUES
(
    1,
    'Cape Sunrise Run',
    'A sunrise route along the promenade.',
    '2026-10-10 06:30:00',
    'Green Point Promenade',
    500,
    'Open'
),
(
    1,
    'Johannesburg Spring Classic',
    'A city road race supporting local schools.',
    '2026-10-24 07:00:00',
    'Zoo Lake Sports Grounds',
    750,
    'Open'
),
(
    2,
    'Drakensberg Trail Day',
    'Scenic mountain trails for experienced runners.',
    '2026-11-07 06:00:00',
    'Royal Natal Park Gate',
    250,
    'Open'
);


/* ============================================================
   11. INSERT EVENT CATEGORIES
   ============================================================ */

INSERT INTO EventCategories (
    EventId,
    CategoryId,
    EntryFee
)
VALUES
(1, 1, 80.00),
(1, 2, 130.00),
(2, 1, 75.00),
(2, 2, 125.00),
(2, 3, 220.00),
(3, 1, 100.00),
(3, 4, 180.00);


/* ============================================================
   12. INSERT ENROLMENTS
   ============================================================ */

INSERT INTO Enrolments (
    EventId,
    ParticipantId,
    CategoryId,
    Status,
    BibNumber
)
VALUES
(1, 3, 2, 'Confirmed', 101),
(1, 4, 1, 'Confirmed', 102),
(2, 3, 3, 'Pending', 201),
(3, 4, 4, 'Confirmed', 301);


/* ============================================================
   13. INSERT RESULTS
   ============================================================ */

INSERT INTO Results (
    EnrolmentId,
    Position,
    FinishTimeSeconds,
    Status
)
VALUES
(1, 2, 3120, 'Published'),
(2, 1, 1685, 'Published');


/* ============================================================
   14. VERIFY THE DATABASE
   ============================================================ */

SELECT * FROM Users;

SELECT * FROM Categories;

SELECT * FROM Events;

SELECT * FROM EventCategories;

SELECT * FROM Enrolments;

SELECT * FROM Results;