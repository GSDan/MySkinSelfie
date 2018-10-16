CREATE TABLE [dbo].[StudyEnrolments]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [StudyId] INT NOT NULL, 
    [UserId] INT NOT NULL, 
    [CreatedAt] DATETIME NOT NULL,
	[Enrolled] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_StudyEnrolments_ToUsers] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_StudyEnrolments_ToStudies] FOREIGN KEY ([StudyId]) REFERENCES [Studies]([Id]) ON DELETE CASCADE
)
