/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
SET IDENTITY_INSERT BodyParts ON;
INSERT INTO BodyParts(Id, Name)
VALUES (1, 'Head'),
		(2, 'Neck'),
		(3, 'Torso'),
		(4, 'Arm'),
		(5, 'Groin'),
		(6, 'Buttock'),
		(7, 'Leg');
SET IDENTITY_INSERT BodyParts OFF;

SET IDENTITY_INSERT SkinRegions ON;
INSERT INTO SkinRegions(Id, Name, BodyPartId)
VALUES  (1, 'Full Face', 1),
		(2, 'Left Side of Face', 1),
		(3, 'Right Side of Face', 1),
		(4, 'Back of Head', 1),
		(5, 'Top of Head', 1),
		(6, 'Front of Neck', 2),
		(7, 'Back of Neck', 2),
		(8, 'Left Side of Neck', 2),
		(9, 'Right Side of Neck', 2),
		(10, 'Left Upper Arm', 4),
		(11, 'Left Forearm', 4),
		(12, 'Left Wrist', 4),
		(13, 'Left Hand', 4),
		(14, 'Right Upper Arm', 4),
		(15, 'Right Forearm', 4),
		(16, 'Right Wrist', 4),
		(17, 'Right Hand', 4),
		(18, 'Groin Area', 5),
		(19, 'Left Thigh', 7),
		(20, 'Left Back of Thigh', 7),
		(21, 'Left Knee', 7),
		(22, 'Left Shin', 7),
		(23, 'Left Calf', 7),
		(24, 'Left Ankle', 7),
		(25, 'Left Foot', 7),
		(26, 'Right Thigh', 7),
		(27, 'Right Back of Thigh', 7),
		(28, 'Right Knee', 7),
		(29, 'Right Shin', 7),
		(30, 'Right Calf', 7),
		(31, 'Right Ankle', 7),
		(32, 'Right Foot', 7),
		(33, 'Chest', 3),
		(34, 'Stomach', 3),
		(35, 'Upper Back', 3),
		(36, 'Lower Back', 3),
		(37, 'Left Side', 3),
		(38, 'Left Shoulder', 3),
		(39, 'Left Shoulder Blade', 3),
		(40, 'Right Side', 3),
		(41, 'Right Shoulder', 3),
		(42, 'Right Shoulder Blade', 3),
		(43, 'Left Buttock', 6),
		(44, 'Right Buttock', 6);
		

SET IDENTITY_INSERT SkinRegions OFF;