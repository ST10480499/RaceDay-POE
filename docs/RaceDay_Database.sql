USE master;
GO
 
IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE RaceDayDB;
END
GO
 
CREATE DATABASE RaceDayDB;
GO
 
USE RaceDayDB;
GO
 

-- 1. ROLES

CREATE TABLE Roles (
RoleID INT IDENTITY(1,1) PRIMARY KEY,
RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO
 

-- 2. USERS

CREATE TABLE Users (
UserID INT IDENTITY(1,1) PRIMARY KEY,
RoleID INT NOT NULL,
FullName NVARCHAR(100) NOT NULL,
Email NVARCHAR(150) NOT NULL UNIQUE,
PasswordHash NVARCHAR(255) NOT NULL,
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO
 

-- 3. EVENTS

CREATE TABLE Events (
EventID INT IDENTITY(1,1) PRIMARY KEY,
OrganiserID INT NOT NULL,
EventName NVARCHAR(150) NOT NULL,
EventDate DATE NOT NULL,
Location NVARCHAR(200) NOT NULL,
CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);
GO
 

-- 4. CATEGORIES

CREATE TABLE Categories (
CategoryID INT IDENTITY(1,1) PRIMARY KEY,
EventID INT NOT NULL,
CategoryName NVARCHAR(50) NOT NULL,
MaxParticipants INT NOT NULL DEFAULT 100,
CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO
 

-- 5. ENROLMENTS

CREATE TABLE Enrolments (
EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
ParticipantID INT NOT NULL,
CategoryID INT NOT NULL,
EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
CONSTRAINT UQ_Enrolments_ParticipantCategory UNIQUE (ParticipantID, CategoryID)
);
GO
 

-- 6. RESULTS

CREATE TABLE Results (
ResultID INT IDENTITY(1,1) PRIMARY KEY,
EnrolmentID INT NOT NULL UNIQUE,
FinishTime TIME NOT NULL,
Position INT NOT NULL,
CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);
GO
 
 

-- SEED DATA

 
-- Roles
INSERT INTO Roles (RoleName) VALUES
('Organiser'),
('Participant');
GO
 
-- Users: 2 Organisers, 2 Participants (minimum required)
INSERT INTO Users (RoleID, FullName, Email, PasswordHash) VALUES
(1, 'Thabo Mokoena', 'thabo.mokoena@raceday.co.za', 'HASHED_PASSWORD_1'),
(1, 'Sarah van Wyk', 'sarah.vanwyk@raceday.co.za', 'HASHED_PASSWORD_2'),
(2, 'Lindiwe Dube', 'lindiwe.dube@example.com', 'HASHED_PASSWORD_3'),
(2, 'James Botha', 'james.botha@example.com', 'HASHED_PASSWORD_4');
GO
 
-- Events: 3 events, created by the 2 Organisers (UserID 1 and 2)
INSERT INTO Events (OrganiserID, EventName, EventDate, Location) VALUES
(1, 'Johannesburg City Marathon', '2026-11-15', 'Sandton, Johannesburg'),
(2, 'Soweto Fun Walk', '2026-10-04', 'Soweto, Johannesburg'),
(1, 'Cape Town Cycle Classic', '2026-12-06', 'Cape Town');
GO
 
-- Categories: at least one category per event
INSERT INTO Categories (EventID, CategoryName, MaxParticipants) VALUES
(1, '10km', 500),
(1, '21km', 300),
(2, '5km Walk', 200),
(3, '50km Cycle', 400),
(3, '100km Cycle', 250);
GO
 
-- Enrolments: participants (UserID 3 and 4) entering categories
INSERT INTO Enrolments (ParticipantID, CategoryID) VALUES
(3, 1), -- Lindiwe enters the 10km
(3, 4), -- Lindiwe enters the 50km Cycle
(4, 2), -- James enters the 21km
(4, 3); -- James enters the 5km Walk
GO
 
-- Results: sample finish times and positions for two enrolments
INSERT INTO Results (EnrolmentID, FinishTime, Position) VALUES
(1, '00:52:30', 1),
(3, '01:45:10', 5);
GO
 
 

-- QUICK VERIFICATION QUERIES (optional - run to check the data)

SELECT * FROM Roles;
SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
