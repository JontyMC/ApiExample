CREATE DATABASE ApiImport

USE ApiImport

CREATE TABLE Listing
(
	Guid NVARCHAR(50),
	Longitude NVARCHAR(50),
	Latitude NVARCHAR(50),
	Summary NVARCHAR(MAX)
)