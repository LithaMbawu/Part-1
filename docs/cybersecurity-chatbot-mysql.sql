CREATE DATABASE IF NOT EXISTS cybersecurity_chatbot
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE cybersecurity_chatbot;

CREATE TABLE IF NOT EXISTS CybersecurityTasks (
    Id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description VARCHAR(2000) NOT NULL,
    ReminderAt DATETIME NULL,
    IsCompleted BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_CybersecurityTasks_IsCompleted (IsCompleted),
    INDEX IX_CybersecurityTasks_ReminderAt (ReminderAt)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;