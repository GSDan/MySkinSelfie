CREATE TABLE [dbo].[SkinRegions]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(MAX) NOT NULL, 
    [BodyPartId] INT NOT NULL, 
    CONSTRAINT [FK_SkinRegions_ToBodyParts] FOREIGN KEY ([BodyPartId]) REFERENCES [BodyParts]([Id])
)
