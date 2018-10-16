CREATE TABLE [dbo].[UserConditions]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SkinRegionId] INT NOT NULL, 
    [OwnerId] INT NOT NULL, 
    [Treatment] NVARCHAR(MAX) NULL, 
    [Condition] NVARCHAR(MAX) NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [Passcode] INT NULL, 
    [Finished] BIT NOT NULL, 
    CONSTRAINT [FK_UserCondition_ToSkinRegions] FOREIGN KEY ([SkinRegionId]) REFERENCES [SkinRegions]([Id]), 
    CONSTRAINT [FK_UserCondition_ToUsers] FOREIGN KEY ([OwnerId]) REFERENCES [Users]([Id])
)