IF DB_ID(N'PuyaqDb') IS NULL
BEGIN
    CREATE DATABASE PuyaqDb;
END;
GO

USE PuyaqDb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'security')
    EXEC(N'CREATE SCHEMA security');
GO

IF OBJECT_ID(N'security.Users', N'U') IS NULL
BEGIN
    CREATE TABLE security.Users
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Email NVARCHAR(256) NOT NULL,
        NormalizedEmail NVARCHAR(256) NOT NULL,
        DisplayName NVARCHAR(150) NOT NULL,
        Status INT NOT NULL,
        LastLoginAt DATETIMEOFFSET NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        UpdatedAt DATETIMEOFFSET NULL,
        CONSTRAINT UQ_Users_NormalizedEmail UNIQUE (NormalizedEmail)
    );
END;
GO

IF OBJECT_ID(N'security.UserCredentials', N'U') IS NULL
BEGIN
    CREATE TABLE security.UserCredentials
    (
        UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserCredentials PRIMARY KEY,
        PasswordHash NVARCHAR(500) NOT NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        UpdatedAt DATETIMEOFFSET NULL,
        CONSTRAINT FK_UserCredentials_Users FOREIGN KEY (UserId)
            REFERENCES security.Users(Id)
    );
END;
GO

IF OBJECT_ID(N'security.UserSessions', N'U') IS NULL
BEGIN
    CREATE TABLE security.UserSessions
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserSessions PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        ExpiresAt DATETIMEOFFSET NOT NULL,
        RevokedAt DATETIMEOFFSET NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId)
            REFERENCES security.Users(Id)
    );
END;
GO

IF OBJECT_ID(N'security.RefreshTokens', N'U') IS NULL
BEGIN
    CREATE TABLE security.RefreshTokens
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        SessionId UNIQUEIDENTIFIER NOT NULL,
        TokenFamilyId UNIQUEIDENTIFIER NOT NULL,
        TokenHash NVARCHAR(128) NOT NULL,
        ExpiresAt DATETIMEOFFSET NOT NULL,
        RevokedAt DATETIMEOFFSET NULL,
        CreatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT UQ_RefreshTokens_TokenHash UNIQUE (TokenHash),
        CONSTRAINT FK_RefreshTokens_UserSessions FOREIGN KEY (SessionId)
            REFERENCES security.UserSessions(Id)
    );
END;
GO

CREATE OR ALTER PROCEDURE security.usp_authentication_get_by_normalized_email
    @NormalizedEmail NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id,
        u.Email,
        u.NormalizedEmail,
        u.DisplayName,
        u.Status,
        c.PasswordHash
    FROM security.Users u
    INNER JOIN security.UserCredentials c ON c.UserId = u.Id
    WHERE u.NormalizedEmail = @NormalizedEmail;
END;
GO

CREATE OR ALTER PROCEDURE security.usp_authentication_register
    @Id UNIQUEIDENTIFIER,
    @Email NVARCHAR(256),
    @NormalizedEmail NVARCHAR(256),
    @DisplayName NVARCHAR(150),
    @Status INT,
    @PasswordHash NVARCHAR(500),
    @CreatedAt DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM security.Users WHERE NormalizedEmail = @NormalizedEmail)
            THROW 50001, 'AUTH_EMAIL_ALREADY_EXISTS', 1;

        INSERT INTO security.Users
            (Id, Email, NormalizedEmail, DisplayName, Status, CreatedAt)
        VALUES
            (@Id, @Email, @NormalizedEmail, @DisplayName, @Status, @CreatedAt);

        INSERT INTO security.UserCredentials
            (UserId, PasswordHash, CreatedAt)
        VALUES
            (@Id, @PasswordHash, @CreatedAt);

        COMMIT TRANSACTION;

        SELECT
            @Id AS Id,
            @Email AS Email,
            @NormalizedEmail AS NormalizedEmail,
            @DisplayName AS DisplayName,
            @Status AS Status,
            @PasswordHash AS PasswordHash;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE security.usp_authentication_update_last_login
    @UserId UNIQUEIDENTIFIER,
    @LastLoginAt DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE security.Users
       SET LastLoginAt = @LastLoginAt,
           UpdatedAt = @LastLoginAt
     WHERE Id = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE security.usp_authentication_save_refresh_token
    @SessionId UNIQUEIDENTIFIER,
    @RefreshTokenId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @TokenFamilyId UNIQUEIDENTIFIER,
    @TokenHash NVARCHAR(128),
    @SessionExpiresAt DATETIMEOFFSET,
    @TokenExpiresAt DATETIMEOFFSET,
    @CreatedAt DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO security.UserSessions (Id, UserId, ExpiresAt, CreatedAt)
        VALUES (@SessionId, @UserId, @SessionExpiresAt, @CreatedAt);

        INSERT INTO security.RefreshTokens
            (Id, SessionId, TokenFamilyId, TokenHash, ExpiresAt, CreatedAt)
        VALUES
            (@RefreshTokenId, @SessionId, @TokenFamilyId, @TokenHash, @TokenExpiresAt, @CreatedAt);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
