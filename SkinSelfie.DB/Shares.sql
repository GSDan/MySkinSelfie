CREATE TABLE [dbo].[Shares]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [CreatedAt] DATETIME NOT NULL, 
    [ExpireDate] DATETIME NOT NULL, 
    [OwnerId] INT NOT NULL, 
    [SharedEmail] NVARCHAR(100) NOT NULL, 
    [ConditionId] INT NOT NULL, 
    [Updated] BIT NOT NULL, 
    CONSTRAINT [FK_Shares_ToUsersOwner] FOREIGN KEY ([OwnerId]) REFERENCES [Users]([Id]), 
    CONSTRAINT [FK_Shares_ToUserConditions] FOREIGN KEY ([ConditionId]) REFERENCES [UserConditions]([Id])
)
