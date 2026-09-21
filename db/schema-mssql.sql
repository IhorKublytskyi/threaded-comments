IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911132008_Init'
)
BEGIN
    IF SCHEMA_ID(N'Core') IS NULL EXEC(N'CREATE SCHEMA [Core];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911132008_Init'
)
BEGIN
    CREATE TABLE [Core].[Comments] (
        [Id] int NOT NULL IDENTITY,
        [ParentCommentId] int NULL,
        [Email] nvarchar(256) NOT NULL,
        [Username] nvarchar(64) NOT NULL,
        [HomePageUrl] nvarchar(256) NULL,
        [Body] nvarchar(1024) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [AttachmentPath] nvarchar(512) NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_Comments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [Core].[Comments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911132008_Init'
)
BEGIN
    CREATE INDEX [IX_Comment_Email] ON [Core].[Comments] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911132008_Init'
)
BEGIN
    CREATE INDEX [IX_Comment_ParentCommentId_CreatedAt] ON [Core].[Comments] ([ParentCommentId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911132008_Init'
)
BEGIN
    CREATE INDEX [IX_Comment_Username] ON [Core].[Comments] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911132008_Init'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260911132008_Init', N'10.0.12');
END;

COMMIT;
GO

