CREATE TABLE [dbo].[Photos]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Url] NVARCHAR(MAX) NOT NULL, 
    [CreatedAt] DATETIME NOT NULL, 
    [Treatment] NVARCHAR(MAX) NULL, 
    [Notes] NVARCHAR(MAX) NULL, 
    [PhotoDescription] NVARCHAR(MAX) NULL, 
    [Rating] INT NULL, 
    [UserConditionId] INT NOT NULL, 
    [ThumbUrl] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [FK_Photos_ToUserConditions] FOREIGN KEY ([UserConditionId]) REFERENCES [UserConditions]([Id]) 
)
