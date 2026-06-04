/*
    2026_ResolveDuplicateFileHashes.sql

    Resolves pre-existing duplicate documents so the unique filtered index
    UX_Documents_Course_FileHash_Active can be created.

    Target database : AcademicRagManagement
    Safe to re-run   : YES (idempotent — already-resolved data produces no changes)

    Run this ONLY when the application startup backfiller
    (DocumentFileHashBackfiller) reported that it could not create the index
    because active documents in the same course share the same real content hash.

    What it does (non-destructive soft delete):
      - Operates on the REAL content hashes already written by the app backfiller
        (rows whose FileHashSha256 is a 64-char hex value; legacy placeholders that
        start with 'legacy' are ignored and never treated as duplicates).
      - For each (CourseId, FileHashSha256) group of active documents
        (UploadStatus <> 'Deleted'), it keeps the EARLIEST document (by CreatedAt,
        then DocumentId) as the canonical one and marks the later duplicates as
        soft-deleted (UploadStatus = 'Deleted'). No rows are physically deleted, so
        chunks, chat sessions and audit logs are preserved.

    After running this, restart the application; the backfiller will then create
    UX_Documents_Course_FileHash_Active.

    Review the SELECT below FIRST to see exactly which rows will be affected.
*/

SET NOCOUNT ON;
GO

/* Preview — rows that WILL be soft-deleted: */
;WITH ranked AS (
    SELECT
        DocumentId, CourseId, FileHashSha256, CreatedAt,
        ROW_NUMBER() OVER (
            PARTITION BY CourseId, FileHashSha256
            ORDER BY CreatedAt, DocumentId) AS rn
    FROM dbo.Documents
    WHERE UploadStatus <> 'Deleted'
      AND FileHashSha256 IS NOT NULL
      AND FileHashSha256 NOT LIKE 'legacy%'      -- only genuine content hashes
)
SELECT DocumentId, CourseId, FileHashSha256, CreatedAt
FROM ranked
WHERE rn > 1
ORDER BY CourseId, FileHashSha256, CreatedAt;
GO

/* Apply — keep earliest active per (CourseId, hash); soft-delete the rest. */
;WITH ranked AS (
    SELECT
        DocumentId,
        ROW_NUMBER() OVER (
            PARTITION BY CourseId, FileHashSha256
            ORDER BY CreatedAt, DocumentId) AS rn
    FROM dbo.Documents
    WHERE UploadStatus <> 'Deleted'
      AND FileHashSha256 IS NOT NULL
      AND FileHashSha256 NOT LIKE 'legacy%'
)
UPDATE d
SET d.UploadStatus = 'Deleted',
    d.UpdatedAt    = SYSUTCDATETIME()
FROM dbo.Documents d
INNER JOIN ranked r ON d.DocumentId = r.DocumentId
WHERE r.rn > 1;
GO
