CREATE TABLE [dbo].[EventLogs] (
    [Id]		INT			   NOT NULL PRIMARY KEY IDENTITY,
    [UserId]    INT            NOT NULL,
    [Action]    NVARCHAR (MAX) NOT NULL,
    [CreatedAt] DATETIME       NOT NULL
);

