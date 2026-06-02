USE AcademicRagManagement;
GO

DECLARE @DefaultCourseId INT;

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Code = N'PRN222')
BEGIN
    INSERT INTO Courses (Code, [Name], [Description], [Status])
    VALUES (N'PRN222', N'Programming with .NET', N'Course documents for PRN222', 1);
END

SELECT @DefaultCourseId = CourseId
FROM Courses
WHERE Code = N'PRN222';

IF COL_LENGTH(N'Accounts', N'CourseId') IS NULL
BEGIN
    ALTER TABLE Accounts ADD CourseId INT NULL;
END
GO

DECLARE @DefaultCourseId INT;

SELECT @DefaultCourseId = CourseId
FROM Courses
WHERE Code = N'PRN222';

UPDATE Accounts
SET CourseId = NULL
WHERE [Role] = 1;

UPDATE Accounts
SET CourseId = @DefaultCourseId
WHERE [Role] = 2
  AND CourseId IS NULL;

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Email = N'teacher@academicrag.org')
BEGIN
    INSERT INTO Accounts (Email, [Password], FullName, [Role], CourseId, [Status])
    VALUES (N'teacher@academicrag.org', N'@@abc123@@', N'Default Teacher', 2, @DefaultCourseId, 1);
END
ELSE
BEGIN
    UPDATE Accounts
    SET CourseId = COALESCE(CourseId, @DefaultCourseId),
        [Status] = 1
    WHERE Email = N'teacher@academicrag.org'
      AND [Role] = 2;
END

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Email = N'student@academicrag.org')
BEGIN
    INSERT INTO Accounts (Email, [Password], FullName, [Role], CourseId, [Status])
    VALUES (N'student@academicrag.org', N'@@abc123@@', N'Default Student', 1, NULL, 1);
END
ELSE
BEGIN
    UPDATE Accounts
    SET CourseId = NULL,
        [Status] = 1
    WHERE Email = N'student@academicrag.org'
      AND [Role] = 1;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE [name] = N'IX_Accounts_CourseId'
      AND object_id = OBJECT_ID(N'Accounts')
)
BEGIN
    CREATE INDEX IX_Accounts_CourseId ON Accounts(CourseId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE [name] = N'FK_Accounts_Courses'
      AND parent_object_id = OBJECT_ID(N'Accounts')
)
BEGIN
    ALTER TABLE Accounts
    ADD CONSTRAINT FK_Accounts_Courses
        FOREIGN KEY (CourseId) REFERENCES Courses(CourseId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE [name] = N'CK_Accounts_TeacherCourse'
      AND parent_object_id = OBJECT_ID(N'Accounts')
)
BEGIN
    ALTER TABLE Accounts
    ADD CONSTRAINT CK_Accounts_TeacherCourse
        CHECK (([Role] = 2 AND CourseId IS NOT NULL) OR ([Role] = 1 AND CourseId IS NULL));
END
GO
