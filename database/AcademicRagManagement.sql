GO

IF DB_ID(N'AcademicRagManagement') IS NOT NULL
BEGIN
    ALTER DATABASE AcademicRagManagement SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AcademicRagManagement;
END
GO

CREATE DATABASE AcademicRagManagement;
GO

USE AcademicRagManagement;
GO

CREATE TABLE Accounts (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL,
    [Password] NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    [Role] INT NOT NULL,
    [Status] BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT UQ_Accounts_Email UNIQUE (Email),
    CONSTRAINT CK_Accounts_Role CHECK ([Role] IN (1, 2))
);
GO

CREATE TABLE Courses (
    CourseId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(1000) NULL,
    [Status] BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT UQ_Courses_Code UNIQUE (Code)
);
GO

CREATE TABLE Documents (
    DocumentId INT IDENTITY(1,1) PRIMARY KEY,

    Title NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(1000) NULL,

    CourseId INT NOT NULL,
    CourseCode NVARCHAR(50) NOT NULL,
    Chapter NVARCHAR(100) NULL,

    OriginalFileName NVARCHAR(255) NOT NULL,
    StoredFileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(1000) NOT NULL,
    FileType NVARCHAR(20) NOT NULL,
    ContentType NVARCHAR(255) NULL,
    FileSize BIGINT NOT NULL DEFAULT 0,

    UploadStatus NVARCHAR(30) NOT NULL DEFAULT 'Submitted',
    IndexStatus NVARCHAR(30) NOT NULL DEFAULT 'Pending',

    TotalChunks INT NOT NULL DEFAULT 0,
    IndexError NVARCHAR(MAX) NULL,

    SubmittedByAccountId INT NULL,
    SubmittedByEmail NVARCHAR(255) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ApprovedAt DATETIME2 NULL,
    IndexedAt DATETIME2 NULL,

    CONSTRAINT FK_Documents_Courses
        FOREIGN KEY (CourseId) REFERENCES Courses(CourseId),

    CONSTRAINT FK_Documents_Accounts
        FOREIGN KEY (SubmittedByAccountId) REFERENCES Accounts(AccountId),

    CONSTRAINT CK_Documents_FileType
        CHECK (FileType IN ('.pdf', '.docx', '.pptx', '.txt')),

    CONSTRAINT CK_Documents_UploadStatus
        CHECK (UploadStatus IN ('Submitted', 'Approved', 'Rejected', 'Deleted')),

    CONSTRAINT CK_Documents_IndexStatus
        CHECK (IndexStatus IN ('Pending', 'Processing', 'Indexed', 'Failed', 'Deleted'))
);
GO

CREATE TABLE ChatSessions (
    ChatSessionId INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL,
    CourseId INT NOT NULL,
    DocumentId INT NOT NULL,
    Title NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_ChatSessions_Accounts
        FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),

    CONSTRAINT FK_ChatSessions_Courses
        FOREIGN KEY (CourseId) REFERENCES Courses(CourseId),

    CONSTRAINT FK_ChatSessions_Documents
        FOREIGN KEY (DocumentId) REFERENCES Documents(DocumentId)
);
GO

CREATE TABLE ChatMessages (
    ChatMessageId INT IDENTITY(1,1) PRIMARY KEY,
    ChatSessionId INT NOT NULL,
    AccountId INT NOT NULL,
    DocumentId INT NOT NULL,

    Question NVARCHAR(MAX) NOT NULL,
    Answer NVARCHAR(MAX) NOT NULL,
    SourcesJson NVARCHAR(MAX) NOT NULL DEFAULT '[]',

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_ChatMessages_ChatSessions
        FOREIGN KEY (ChatSessionId) REFERENCES ChatSessions(ChatSessionId),

    CONSTRAINT FK_ChatMessages_Accounts
        FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),

    CONSTRAINT FK_ChatMessages_Documents
        FOREIGN KEY (DocumentId) REFERENCES Documents(DocumentId)
);
GO

CREATE INDEX IX_Accounts_Email ON Accounts(Email);
CREATE INDEX IX_Courses_Code ON Courses(Code);

CREATE INDEX IX_Documents_CourseId ON Documents(CourseId);
CREATE INDEX IX_Documents_CourseCode ON Documents(CourseCode);
CREATE INDEX IX_Documents_SubmittedByAccountId ON Documents(SubmittedByAccountId);
CREATE INDEX IX_Documents_UploadStatus ON Documents(UploadStatus);
CREATE INDEX IX_Documents_IndexStatus ON Documents(IndexStatus);

CREATE INDEX IX_ChatSessions_AccountId ON ChatSessions(AccountId);
CREATE INDEX IX_ChatSessions_DocumentId ON ChatSessions(DocumentId);

CREATE INDEX IX_ChatMessages_ChatSessionId ON ChatMessages(ChatSessionId);
CREATE INDEX IX_ChatMessages_DocumentId ON ChatMessages(DocumentId);
CREATE INDEX IX_ChatMessages_CreatedAt ON ChatMessages(CreatedAt);
GO

INSERT INTO Courses (Code, [Name], [Description])
VALUES
('PRN222', 'Programming with .NET', 'Course documents for PRN222');
GO

INSERT INTO Accounts (Email, [Password], FullName, [Role], [Status])
VALUES
('teacher@academicrag.org', '@@abc123@@', 'Default Teacher', 2, 1),
('student@academicrag.org', '@@abc123@@', 'Default Student', 1, 1);
GO