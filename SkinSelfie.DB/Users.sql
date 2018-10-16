CREATE TABLE [dbo].[Users]
(
	[Id] INT NOT NULL IDENTITY , 
    [Name] NVARCHAR(MAX) NOT NULL, 
    [BirthDate] DATETIME NOT NULL, 
    [Email] NVARCHAR(100) UNIQUE NOT NULL, 
    [Admin] BIT NULL, 
    PRIMARY KEY ([Id])
)

GO
