using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcademicDocumentRagSystem.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AcademicDocumentRagSystem.Services.Maintenance
{
    /// <summary>
    /// One-time, idempotent startup migration that gives every existing Documents
    /// row a real SHA-256 content hash and then creates the unique filtered index
    /// that blocks duplicate uploads at the database level.
    ///
    /// Why this exists: a pure-SQL migration cannot read file bytes off disk to
    /// hash them, so backfilling legacy rows with random placeholders would make
    /// those documents impossible to detect as duplicates. This service reads each
    /// document's physical file (Documents.FilePath), computes the real hash, and
    /// only falls back to a unique placeholder when the file is missing/unreadable.
    ///
    /// It deliberately reads/writes through raw ADO.NET rather than the EF entity
    /// model: when the column has just been added it is still NULL for legacy rows,
    /// and materialising the non-nullable Document.FileHashSha256 property would
    /// throw. Raw reads tolerate the transient NULLs.
    ///
    /// Safety: it never deletes or soft-deletes user data. If pre-existing active
    /// duplicates would violate the unique index, it logs the offending DocumentIds
    /// plus resolution instructions and leaves the index uncreated. New uploads are
    /// still blocked by the service-level duplicate check in DocumentService.
    /// </summary>
    public class DocumentFileHashBackfiller
    {
        private const string IndexName = "UX_Documents_Course_FileHash_Active";

        private static readonly Regex RealHashPattern =
            new("^[0-9a-f]{64}$", RegexOptions.Compiled);

        private readonly AcademicRagDbContext _context;
        private readonly ILogger<DocumentFileHashBackfiller> _logger;

        public DocumentFileHashBackfiller(
            AcademicRagDbContext context,
            ILogger<DocumentFileHashBackfiller> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            // Self-heal the schema so the app works even if the SQL script was not
            // applied manually. This must run before any EF query touches Documents.
            await EnsureColumnAsync();

            // The index only exists once a clean backfill has completed, so its
            // presence is our "already migrated" marker.
            if (await IndexExistsAsync())
            {
                return;
            }

            var rows = await LoadRowsAsync();

            var realCount = 0;
            var placeholderCount = 0;
            var updates = new List<(int DocumentId, string Hash)>();

            foreach (var row in rows)
            {
                if (IsRealHash(row.FileHashSha256))
                {
                    continue; // already carries a real content hash
                }

                var hash = TryComputeFileHash(row.FilePath);
                if (hash != null)
                {
                    realCount++;
                }
                else
                {
                    hash = CreatePlaceholder();
                    placeholderCount++;
                    _logger.LogWarning(
                        "DocumentFileHashBackfiller: physical file missing/unreadable for DocumentId {DocumentId} " +
                        "(path: {Path}). Assigned a unique placeholder hash; duplicate detection is unavailable for " +
                        "this legacy document until it is re-uploaded.",
                        row.DocumentId, row.FilePath);
                }

                row.FileHashSha256 = hash;
                updates.Add((row.DocumentId, hash));
            }

            await ApplyHashUpdatesAsync(updates);

            // Safe now that every row has a value.
            await EnforceNotNullAsync();

            var activeDuplicateGroups = rows
                .Where(r => r.UploadStatus != "Deleted" && IsRealHash(r.FileHashSha256))
                .GroupBy(r => new { r.CourseId, r.FileHashSha256 })
                .Where(g => g.Count() > 1)
                .ToList();

            if (activeDuplicateGroups.Count > 0)
            {
                foreach (var group in activeDuplicateGroups)
                {
                    var ordered = group
                        .OrderBy(r => r.CreatedAt)
                        .ThenBy(r => r.DocumentId)
                        .ToList();

                    var canonical = ordered.First();
                    var laterIds = string.Join(", ", ordered.Skip(1).Select(r => r.DocumentId));

                    _logger.LogError(
                        "DocumentFileHashBackfiller: CourseId {CourseId} has multiple active documents with identical " +
                        "content hash {Hash}. Earliest (canonical) = DocumentId {Canonical}; later duplicates = [{LaterIds}]. " +
                        "No data was changed.",
                        group.Key.CourseId, group.Key.FileHashSha256, canonical.DocumentId, laterIds);
                }

                _logger.LogError(
                    "DocumentFileHashBackfiller: unique index {Index} was NOT created because pre-existing active " +
                    "duplicates exist. Resolve them by soft-deleting the later duplicates (set UploadStatus='Deleted') " +
                    "— run DatabaseScripts/2026_ResolveDuplicateFileHashes.sql — then restart the app. New uploads remain " +
                    "blocked by the service-level duplicate check in the meantime.",
                    IndexName);

                return;
            }

            await CreateIndexAsync();

            _logger.LogInformation(
                "DocumentFileHashBackfiller: backfill complete. Real hashes computed: {Real}, placeholders: {Placeholder}. " +
                "Unique index {Index} created.",
                realCount, placeholderCount, IndexName);
        }

        private static bool IsRealHash(string? value) =>
            !string.IsNullOrWhiteSpace(value) && RealHashPattern.IsMatch(value);

        private static string? TryComputeFileHash(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    return null;
                }

                using var stream = File.OpenRead(path);
                using var sha = SHA256.Create();
                var bytes = sha.ComputeHash(stream);
                return Convert.ToHexString(bytes).ToLowerInvariant();
            }
            catch
            {
                // Unreadable file (locked, permissions, etc.) -> placeholder fallback.
                return null;
            }
        }

        private static string CreatePlaceholder()
        {
            // Starts with "legacy" so it can never collide with a real lowercase-hex
            // SHA-256, and is unique per row so it never blocks the unique index.
            var raw = "legacy" + Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            return raw.Substring(0, 64);
        }

        private async Task<List<DocumentHashRow>> LoadRowsAsync()
        {
            var rows = new List<DocumentHashRow>();
            var connection = _context.Database.GetDbConnection();
            var wasClosed = connection.State != ConnectionState.Open;

            if (wasClosed)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT DocumentId, CourseId, UploadStatus, CreatedAt, FileHashSha256, FilePath FROM dbo.Documents";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new DocumentHashRow
                    {
                        DocumentId = reader.GetInt32(0),
                        CourseId = reader.GetInt32(1),
                        UploadStatus = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3),
                        // CHAR(64) is space-padded; trim so hex comparison is exact.
                        FileHashSha256 = reader.IsDBNull(4) ? null : reader.GetString(4).Trim(),
                        FilePath = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
            finally
            {
                if (wasClosed)
                {
                    await connection.CloseAsync();
                }
            }

            return rows;
        }

        private async Task ApplyHashUpdatesAsync(List<(int DocumentId, string Hash)> updates)
        {
            if (updates.Count == 0)
            {
                return;
            }

            var connection = _context.Database.GetDbConnection();
            var wasClosed = connection.State != ConnectionState.Open;

            if (wasClosed)
            {
                await connection.OpenAsync();
            }

            try
            {
                foreach (var (documentId, hash) in updates)
                {
                    using var command = connection.CreateCommand();
                    command.CommandText =
                        "UPDATE dbo.Documents SET FileHashSha256 = @hash WHERE DocumentId = @id";

                    var hashParam = command.CreateParameter();
                    hashParam.ParameterName = "@hash";
                    hashParam.Value = hash;
                    command.Parameters.Add(hashParam);

                    var idParam = command.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = documentId;
                    command.Parameters.Add(idParam);

                    await command.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (wasClosed)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private async Task EnsureColumnAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Documents') AND name = N'FileHashSha256')
    ALTER TABLE dbo.Documents ADD FileHashSha256 CHAR(64) NULL;");
        }

        private async Task EnforceNotNullAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Documents') AND name = N'FileHashSha256' AND is_nullable = 1)
   AND NOT EXISTS (SELECT 1 FROM dbo.Documents WHERE FileHashSha256 IS NULL)
    ALTER TABLE dbo.Documents ALTER COLUMN FileHashSha256 CHAR(64) NOT NULL;");
        }

        private async Task<bool> IndexExistsAsync()
        {
            var counts = await _context.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value FROM sys.indexes " +
                    "WHERE name = N'UX_Documents_Course_FileHash_Active' AND object_id = OBJECT_ID(N'dbo.Documents')")
                .ToListAsync();

            return counts.FirstOrDefault() > 0;
        }

        private async Task CreateIndexAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Documents_Course_FileHash_Active' AND object_id = OBJECT_ID(N'dbo.Documents'))
    CREATE UNIQUE INDEX UX_Documents_Course_FileHash_Active
        ON dbo.Documents (CourseId, FileHashSha256)
        WHERE UploadStatus <> 'Deleted';");
        }

        private sealed class DocumentHashRow
        {
            public int DocumentId { get; init; }
            public int CourseId { get; init; }
            public string UploadStatus { get; init; } = string.Empty;
            public DateTime CreatedAt { get; init; }
            public string? FileHashSha256 { get; set; }
            public string? FilePath { get; init; }
        }
    }
}
