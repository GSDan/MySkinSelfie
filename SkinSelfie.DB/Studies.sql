CREATE TABLE [dbo].[Studies]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(MAX) NOT NULL, 
    [Code] NVARCHAR(50) NOT NULL, 
    [CreatedAt] DATETIME NOT NULL, 
    [ManagerId] INT NOT NULL,
	[Active] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_Studies_ToUsers] FOREIGN KEY ([ManagerId]) REFERENCES [Users]([Id])
)
