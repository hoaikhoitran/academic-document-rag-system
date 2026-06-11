/*
    2026_AddDocumentChunksAndIndexLogs.sql

    Adds chunk-preview storage and document audit/index logging for the
    Academic Document RAG System.

    Target database : AcademicRagManagement
    Safe to re-run   : YES (all objects are guarded with IF NOT EXISTS checks)

    This script ONLY adds new tables/indexes/constraints. It does not modify
    or drop any existing column on Documents, Accounts or Courses.

    It matches the EF Core configuration in
    AcademicRagDbContext.OnModelCreating exactly.
*/

SET NOCOUNT ON;
GO

/* ------------------------------------------------------------------ */
/* 1. DocumentChunks                                                  */
/* ------------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'DocumentChunks' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.DocumentChunks
    (
        DocumentChunkId INT IDENTITY(1,1) NOT NULL,
        DocumentId      INT            NOT NULL,
        ChunkIndex      INT            NOT NULL,
        PageNumber      INT            NULL,
        ChunkText       NVARCHAR(MAX)  NOT NULL,
        CharCount       INT            NOT NULL,
        TokenEstimate   INT            NULL,
        CreatedAt       DATETIME2      NOT NULL
            CONSTRAINT DF_DocumentChunks_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_DocumentChunks PRIMARY KEY CLUSTERED (DocumentChunkId)
    );
END;
GO

/* FK DocumentChunks.DocumentId -> Documents(DocumentId) */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DocumentChunks_Documents')
BEGIN
    ALTER TABLE dbo.DocumentChunks WITH CHECK
        ADD CONSTRAINT FK_DocumentChunks_Documents
        FOREIGN KEY (DocumentId) REFERENCES dbo.Documents (DocumentId)
        ON DELETE CASCADE;
END;
GO

/* UNIQUE(DocumentId, ChunkIndex) */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_DocumentChunks_DocumentId_ChunkIndex' AND object_id = OBJECT_ID(N'dbo.DocumentChunks'))
BEGIN
    CREATE UNIQUE INDEX UQ_DocumentChunks_DocumentId_ChunkIndex
        ON dbo.DocumentChunks (DocumentId, ChunkIndex);
END;
GO

/* IX_DocumentChunks_DocumentId */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentChunks_DocumentId' AND object_id = OBJECT_ID(N'dbo.DocumentChunks'))
BEGIN
    CREATE INDEX IX_DocumentChunks_DocumentId
        ON dbo.DocumentChunks (DocumentId);
END;
GO

/* IX_DocumentChunks_DocumentId_ChunkIndex */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentChunks_DocumentId_ChunkIndex' AND object_id = OBJECT_ID(N'dbo.DocumentChunks'))
BEGIN
    CREATE INDEX IX_DocumentChunks_DocumentId_ChunkIndex
        ON dbo.DocumentChunks (DocumentId, ChunkIndex);
END;
GO

/* ------------------------------------------------------------------ */
/* 2. DocumentIndexLogs                                               */
/* ------------------------------------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'DocumentIndexLogs' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.DocumentIndexLogs
    (
        DocumentIndexLogId   INT IDENTITY(1,1) NOT NULL,
        DocumentId           INT            NOT NULL,
        Action               NVARCHAR(30)   NOT NULL,   -- Upload, Index, ReIndex, Update, Delete, Preview
        Status               NVARCHAR(30)   NOT NULL,   -- Success, Failed
        PerformedByAccountId INT            NULL,        -- Admin from appsettings has no AccountId -> NULL
        PerformedByEmail     NVARCHAR(255)  NOT NULL,
        PerformedAt          DATETIME2      NOT NULL
            CONSTRAINT DF_DocumentIndexLogs_PerformedAt DEFAULT (SYSUTCDATETIME()),
        TotalChunks          INT            NULL,
        ErrorMessage         NVARCHAR(MAX)  NULL,
        CONSTRAINT PK_DocumentIndexLogs PRIMARY KEY CLUSTERED (DocumentIndexLogId)
    );
END;
GO

/* FK DocumentIndexLogs.DocumentId -> Documents(DocumentId) */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DocumentIndexLogs_Documents')
BEGIN
    ALTER TABLE dbo.DocumentIndexLogs WITH CHECK
        ADD CONSTRAINT FK_DocumentIndexLogs_Documents
        FOREIGN KEY (DocumentId) REFERENCES dbo.Documents (DocumentId);
END;
GO

/* FK DocumentIndexLogs.PerformedByAccountId -> Accounts(AccountId), nullable */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DocumentIndexLogs_Accounts')
BEGIN
    ALTER TABLE dbo.DocumentIndexLogs WITH CHECK
        ADD CONSTRAINT FK_DocumentIndexLogs_Accounts
        FOREIGN KEY (PerformedByAccountId) REFERENCES dbo.Accounts (AccountId);
END;
GO

/* IX_DocumentIndexLogs_DocumentId */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentIndexLogs_DocumentId' AND object_id = OBJECT_ID(N'dbo.DocumentIndexLogs'))
BEGIN
    CREATE INDEX IX_DocumentIndexLogs_DocumentId
        ON dbo.DocumentIndexLogs (DocumentId);
END;
GO

/* IX_DocumentIndexLogs_PerformedAt */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentIndexLogs_PerformedAt' AND object_id = OBJECT_ID(N'dbo.DocumentIndexLogs'))
BEGIN
    CREATE INDEX IX_DocumentIndexLogs_PerformedAt
        ON dbo.DocumentIndexLogs (PerformedAt);
END;
GO

/* IX_DocumentIndexLogs_PerformedByAccountId */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentIndexLogs_PerformedByAccountId' AND object_id = OBJECT_ID(N'dbo.DocumentIndexLogs'))
BEGIN
    CREATE INDEX IX_DocumentIndexLogs_PerformedByAccountId
        ON dbo.DocumentIndexLogs (PerformedByAccountId);
END;
GO
