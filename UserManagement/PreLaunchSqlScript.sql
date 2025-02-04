CREATE DATABASE UserManagementDb
GO

USE [UserManagementDb]
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Immutable ID, auto-generated
    UserName NVARCHAR(255) UNIQUE NOT NULL,
    FullName NVARCHAR(255),
    Email NVARCHAR(255),
    MobileNumber NVARCHAR(20),
    Language NVARCHAR(50),
    Culture NVARCHAR(50),
    PasswordHash NVARCHAR(500) NOT NULL -- Store hashed password here
)
GO

CREATE TABLE Clients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) UNIQUE NOT NULL,
    ApiKey NVARCHAR(500) UNIQUE NOT NULL
)
GO

INSERT INTO [dbo].[Clients]
           ([Name]
           ,[ApiKey])
     VALUES
           ('DefaultClient'
           ,'DefaultClientApiKey')
GO