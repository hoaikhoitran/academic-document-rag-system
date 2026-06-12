/*
    2026_AddDocumentFileHash.sql

    Adds content-hash based duplicate-upload protection for the Academic
    Document RAG System.

    Target database : AcademicRagManagement
    Safe to re-run   : YES (guarded with IF NOT EXISTS)

    IMPORTANT — this script ONLY adds the nullable column.
    It deliberately does NOT backfill placeholder hashes and does NOT create
    the unique index. Real SHA-256 content hashes for existing rows are computed
    from the physical files by the application on startup
    (AcademicDocumentRagSystem.Services.Maintenance.DocumentFileHashBackfiller),
    which then:
      - enforces NOT NULL once every row has a value,
      - creates the unique filtered index UX_Documents_Course_FileHash_Active,
      - or, if pre-existing active duplicates are found, logs clear instructions
        and leaves the index uncreated WITHOUT changing any user data.

    Using random placeholders for every legacy row (the previous approach) would
    have made old documents undetectable as duplicates. Computing the real file
    hash fixes that.

    If you prefer not to rely on application startup, you may still run this
    script to add the column; the application will finish the backfill + index
    on its next run regardless.
*/

SET NOCOUNT ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Documents') AND name = N'FileHashSha256')
BEGIN
    ALTER TABLE dbo.Documents ADD FileHashSha256 CHAR(64) NULL;
END;
GO
