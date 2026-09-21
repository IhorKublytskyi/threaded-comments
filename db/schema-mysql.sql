-- Schema exported for review in MySQL Workbench.
-- Runtime uses MS SQL Server (see schema-mssql.sql for the authoritative DDL).
-- Types are mapped to MySQL equivalents; structure, indexes and FK mirror the EF Core model.

CREATE SCHEMA IF NOT EXISTS `Core`;

CREATE TABLE `Core`.`Comments` (
  `Id`              INT           NOT NULL AUTO_INCREMENT,
  `ParentCommentId` INT           NULL,
  `Email`           VARCHAR(256)  NOT NULL,
  `Username`        VARCHAR(64)   NOT NULL,
  `HomePageUrl`     VARCHAR(256)  NULL,
  `Body`            VARCHAR(1024) NOT NULL,
  `CreatedAt`       DATETIME(6)   NOT NULL,
  `AttachmentPath`  VARCHAR(512)  NULL,

  CONSTRAINT `PK_Comments` PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Comments_Comments_ParentCommentId`
    FOREIGN KEY (`ParentCommentId`)
    REFERENCES `Core`.`Comments` (`Id`)
    ON DELETE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX `IX_Comment_Email`
  ON `Core`.`Comments` (`Email`);

CREATE INDEX `IX_Comment_ParentCommentId_CreatedAt`
  ON `Core`.`Comments` (`ParentCommentId`, `CreatedAt`);

CREATE INDEX `IX_Comment_Username`
  ON `Core`.`Comments` (`Username`);