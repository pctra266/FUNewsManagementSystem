USE [master]
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'FUNewsManagement')
BEGIN
    ALTER DATABASE [FUNewsManagement] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [FUNewsManagement];
END
GO

CREATE DATABASE [FUNewsManagement]
GO

USE [FUNewsManagement]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1. Table: Category
CREATE TABLE [dbo].[Category](
	[CategoryID] [smallint] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](100) NOT NULL,
	[CategoryDescription] [nvarchar](250) NOT NULL,
	[ParentCategoryID] [smallint] NULL,
	[IsActive] [bit] DEFAULT 1 NULL,
 CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([CategoryID] ASC)
) ON [PRIMARY]
GO

-- 2. Table: SystemAccount
CREATE TABLE [dbo].[SystemAccount](
	[AccountID] [smallint] IDENTITY(1,1) NOT NULL,
	[AccountName] [nvarchar](100) NULL,
	[AccountEmail] [nvarchar](70) NULL,
	[AccountRole] [int] NULL,
	[AccountPassword] [nvarchar](70) NULL,
	[RefreshToken] [nvarchar](200) NULL,
	[RefreshTokenExpiryTime] [datetime] NULL,
 CONSTRAINT [PK_SystemAccount] PRIMARY KEY CLUSTERED ([AccountID] ASC)
) ON [PRIMARY]
GO

-- 3. Table: NewsArticle
CREATE TABLE [dbo].[NewsArticle](
	[NewsArticleID] [nvarchar](20) NOT NULL,
	[NewsTitle] [nvarchar](400) NULL,
	[Headline] [nvarchar](150) NOT NULL,
	[CreatedDate] [datetime] DEFAULT GETDATE() NULL,
	[NewsContent] [nvarchar](4000) NULL,
	[NewsSource] [nvarchar](400) NULL,
	[CategoryID] [smallint] NULL,
	[NewsStatus] [bit] DEFAULT 1 NULL,
	[CreatedByID] [smallint] NULL,
	[UpdatedByID] [smallint] NULL,
	[ModifiedDate] [datetime] DEFAULT GETDATE() NULL,
	[ViewCount] [int] DEFAULT 0 NOT NULL,
 CONSTRAINT [PK_NewsArticle] PRIMARY KEY CLUSTERED ([NewsArticleID] ASC)
) ON [PRIMARY]
GO

-- 4. Table: Tag
CREATE TABLE [dbo].[Tag](
	[TagID] [int] IDENTITY(1,1) NOT NULL,
	[TagName] [nvarchar](50) NULL,
	[Note] [nvarchar](400) NULL,
    [IsActive] [bit] DEFAULT 1 NULL,
 CONSTRAINT [PK_HashTag] PRIMARY KEY CLUSTERED ([TagID] ASC)
) ON [PRIMARY]
GO

-- 5. Table: NewsTag
CREATE TABLE [dbo].[NewsTag](
	[NewsArticleID] [nvarchar](20) NOT NULL,
	[TagID] [int] NOT NULL,
 CONSTRAINT [PK_NewsTag] PRIMARY KEY CLUSTERED ([NewsArticleID] ASC, [TagID] ASC)
) ON [PRIMARY]
GO

-- 6. Table: AuditLog
CREATE TABLE [dbo].[AuditLog](
    [LogId] INT IDENTITY(1,1) NOT NULL,
    [UserId] SMALLINT NOT NULL,
    [Action] NVARCHAR(50) NOT NULL,
    [EntityName] NVARCHAR(100) NOT NULL,
    [EntityId] NVARCHAR(50) NOT NULL,
    [OldValues] NVARCHAR(MAX) NULL,
    [NewValues] NVARCHAR(MAX) NULL,
    [Timestamp] DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED ([LogId] ASC)
) ON [PRIMARY]
GO

-- CONSTRAINTS & FOREIGN KEYS

ALTER TABLE [dbo].[Category]  WITH CHECK ADD  CONSTRAINT [FK_Category_Category] FOREIGN KEY([ParentCategoryID])
REFERENCES [dbo].[Category] ([CategoryID])
GO
ALTER TABLE [dbo].[Category] CHECK CONSTRAINT [FK_Category_Category]
GO

ALTER TABLE [dbo].[NewsArticle]  WITH CHECK ADD  CONSTRAINT [FK_NewsArticle_Category] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[Category] ([CategoryID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NewsArticle] CHECK CONSTRAINT [FK_NewsArticle_Category]
GO

ALTER TABLE [dbo].[NewsArticle]  WITH CHECK ADD  CONSTRAINT [FK_NewsArticle_SystemAccount] FOREIGN KEY([CreatedByID])
REFERENCES [dbo].[SystemAccount] ([AccountID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NewsArticle] CHECK CONSTRAINT [FK_NewsArticle_SystemAccount]
GO

ALTER TABLE [dbo].[NewsArticle]  WITH CHECK ADD  CONSTRAINT [FK_NewsArticle_SystemAccount_Updated] FOREIGN KEY([UpdatedByID])
REFERENCES [dbo].[SystemAccount] ([AccountID])
GO
ALTER TABLE [dbo].[NewsArticle] CHECK CONSTRAINT [FK_NewsArticle_SystemAccount_Updated]
GO

ALTER TABLE [dbo].[NewsTag]  WITH CHECK ADD  CONSTRAINT [FK_NewsTag_NewsArticle] FOREIGN KEY([NewsArticleID])
REFERENCES [dbo].[NewsArticle] ([NewsArticleID])
GO
ALTER TABLE [dbo].[NewsTag] CHECK CONSTRAINT [FK_NewsTag_NewsArticle]
GO

ALTER TABLE [dbo].[NewsTag]  WITH CHECK ADD  CONSTRAINT [FK_NewsTag_Tag] FOREIGN KEY([TagID])
REFERENCES [dbo].[Tag] ([TagID])
GO
ALTER TABLE [dbo].[NewsTag] CHECK CONSTRAINT [FK_NewsTag_Tag]
GO

ALTER TABLE [dbo].[AuditLog] WITH CHECK ADD CONSTRAINT [FK_AuditLog_SystemAccount] FOREIGN KEY([UserId])
REFERENCES [dbo].[SystemAccount] ([AccountID])
GO
ALTER TABLE [dbo].[AuditLog] CHECK CONSTRAINT [FK_AuditLog_SystemAccount]
GO

-- 7. Table: NewsArticleImage
CREATE TABLE [dbo].[NewsArticleImage](
	[ImageID] [int] IDENTITY(1,1) NOT NULL,
	[NewsArticleID] [nvarchar](20) NOT NULL,
	[ImageURL] [nvarchar](400) NOT NULL,
	[Caption] [nvarchar](250) NULL,
    [CreatedDate] [datetime] DEFAULT GETDATE() NULL,
 CONSTRAINT [PK_NewsArticleImage] PRIMARY KEY CLUSTERED ([ImageID] ASC)
) ON [PRIMARY]
GO

-- Constraint for NewsArticleImage
ALTER TABLE [dbo].[NewsArticleImage] WITH CHECK ADD CONSTRAINT [FK_NewsArticleImage_NewsArticle] FOREIGN KEY([NewsArticleID])
REFERENCES [dbo].[NewsArticle] ([NewsArticleID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NewsArticleImage] CHECK CONSTRAINT [FK_NewsArticleImage_NewsArticle]
GO

USE [master]
GO
ALTER DATABASE [FUNewsManagement] SET  READ_WRITE 
GO

