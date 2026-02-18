USE [FUNewsManagement]
GO

-- 1. SEED DATA FOR CATEGORY
SET IDENTITY_INSERT [dbo].[Category] ON 
GO
INSERT [dbo].[Category] ([CategoryID], [CategoryName], [CategoryDescription], [ParentCategoryID], [IsActive]) VALUES (1, N'Academic news', N'This category can include articles about research findings, faculty appointments and promotions, and other academic-related announcements.', 1, 1)
INSERT [dbo].[Category] ([CategoryID], [CategoryName], [CategoryDescription], [ParentCategoryID], [IsActive]) VALUES (2, N'Student Affairs', N'This category can include articles about student activities, events, and initiatives, such as student clubs, organizations and sports.', 2, 1)
INSERT [dbo].[Category] ([CategoryID], [CategoryName], [CategoryDescription], [ParentCategoryID], [IsActive]) VALUES (3, N'Campus Safety', N'This category can include articles about incidents and safety measures implemented on campus to ensure the safety of students and faculty.', 3, 1)
INSERT [dbo].[Category] ([CategoryID], [CategoryName], [CategoryDescription], [ParentCategoryID], [IsActive]) VALUES (4, N'Alumni News', N'This category can include articles about the achievements and accomplishments of former students and alumni, such as graduations, job promotions and career successes.', 4, 1)
INSERT [dbo].[Category] ([CategoryID], [CategoryName], [CategoryDescription], [ParentCategoryID], [IsActive]) VALUES (5, N'Capstone Project News', N'This category is typically a comprehensive and detailed report created as part of an academic or professional capstone project. ', 5, 0)
GO
SET IDENTITY_INSERT [dbo].[Category] OFF
GO

-- 2. SEED DATA FOR SYSTEM ACCOUNT
SET IDENTITY_INSERT [dbo].[SystemAccount] ON 
GO
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (1, N'Emma William', N'EmmaWilliam@FUNewsManagement.org', 2, N'@1')
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (2, N'Olivia James', N'OliviaJames@FUNewsManagement.org', 2, N'@1')
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (3, N'Isabella David', N'IsabellaDavid@FUNewsManagement.org', 1, N'@1')
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (4, N'Michael Charlotte', N'MichaelCharlotte@FUNewsManagement.org', 1, N'@1')
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (5, N'Steve Paris', N'SteveParis@FUNewsManagement.org', 1, N'@1')
-- From Datatest.sql
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (6, N'John Editor', N'john.editor@funews.org', 1, N'@1')
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (7, N'Sarah Staff', N'sarah.staff@funews.org', 2, N'@1')
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (8, N'Robert Lecturer', N'robert.lec@funews.org', 2, N'@1')
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (9, N'Admin Root', N'admin@funews.org', 0, N'@1')
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) VALUES (10, N'Guest Writer', N'guest@funews.org', 2, N'@1')
GO
SET IDENTITY_INSERT [dbo].[SystemAccount] OFF
GO
GO

-- 3. SEED DATA FOR TAG
SET IDENTITY_INSERT [dbo].[Tag] ON 
GO
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (1, N'Education', N'Education Note')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (2, N'Technology', N'Technology Note')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (3, N'Research', N'Research Note')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (4, N'Innovation', N'Innovation Note')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (5, N'Campus Life', N'Campus Life Note')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (6, N'Faculty', N'Faculty Achievements')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (7, N'Alumni ', N'Alumni News')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (8, N'Events', N'University Events')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (9, N'Resources', N'Campus Resources')
-- From Datatest.sql
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (10, N'Scholarship', N'Scholarship Opportunities')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (11, N'Start-up', N'Student Start-ups')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (12, N'Sports', N'Football and physical activities')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (13, N'Examination', N'Mid-term and Final Exams')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (14, N'Career', N'Job opportunities')
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) VALUES (15, N'Clubs', N'FPTU Clubs')
GO
SET IDENTITY_INSERT [dbo].[Tag] OFF
GO
GO

-- 4. SEED DATA FOR NEWS ARTICLE
INSERT [dbo].[NewsArticle] ([NewsArticleID], [NewsTitle], [Headline], [CreatedDate], [NewsContent], [NewsSource], [CategoryID], [NewsStatus], [CreatedByID], [UpdatedByID], [ModifiedDate]) VALUES (N'1', N'University FU Celebrates Success of Alumni in Various Fields', N'University FU Celebrates Success of Alumni in Various Fields', CAST(N'2024-05-05T00:00:00.000' AS DateTime), N'University FU recently commemorated the achievements of its esteemed alumni who have excelled in a multitude of fields, showcasing the impact of the institution''s education on their professional journeys.', N'N/A', 4, 1, 1, 1, CAST(N'2024-05-05T00:00:00.000' AS DateTime))
INSERT [dbo].[NewsArticle] ([NewsArticleID], [NewsTitle], [Headline], [CreatedDate], [NewsContent], [NewsSource], [CategoryID], [NewsStatus], [CreatedByID], [UpdatedByID], [ModifiedDate]) VALUES (N'2', N'Alumni Association Launches Mentorship Program for Recent Graduates', N'Alumni Association Launches Mentorship Program for Recent Graduates', CAST(N'2024-05-05T00:00:00.000' AS DateTime), N'The Alumni Association of University FU recently unveiled a new mentorship program aimed at providing support and guidance to recent graduates as they navigate the transition from academia to the professional world.', N'Internet', 4, 1, 1, 1, CAST(N'2024-05-05T00:00:00.000' AS DateTime))
INSERT [dbo].[NewsArticle] ([NewsArticleID], [NewsTitle], [Headline], [CreatedDate], [NewsContent], [NewsSource], [CategoryID], [NewsStatus], [CreatedByID], [UpdatedByID], [ModifiedDate]) VALUES (N'3', N'Academic Department Announces Groundbreaking Initiatives and Program Enhancements', N'Academic Department Announces Groundbreaking Initiatives and Program Enhancements', CAST(N'2024-05-05T00:00:00.000' AS DateTime), N'The Software Engineering Department at FU has unveiled a series of transformative initiatives and program enhancements aimed at enriching the academic experience and fostering innovation in Software Development.', N'N/A', 1, 1, 2, 2, CAST(N'2024-05-05T00:00:00.000' AS DateTime))
INSERT [dbo].[NewsArticle] ([NewsArticleID], [NewsTitle], [Headline], [CreatedDate], [NewsContent], [NewsSource], [CategoryID], [NewsStatus], [CreatedByID], [UpdatedByID], [ModifiedDate]) VALUES (N'4', N'Renowned Scholar Appointed as Head of AI Department at FU', N'Renowned Scholar Appointed as Head of AI Department at FU', CAST(N'2024-05-05T00:00:00.000' AS DateTime), N'FU proudly announces the appointment of David Nitzevet, a distinguished scholar in Machine Learning, to the prestigious position of Head of AI Department, underscoring the institution''s commitment to academic excellence and leadership.', N'N/A', 1, 1, 2, 2, CAST(N'2024-05-05T00:00:00.000' AS DateTime))
INSERT [dbo].[NewsArticle] ([NewsArticleID], [NewsTitle], [Headline], [CreatedDate], [NewsContent], [NewsSource], [CategoryID], [NewsStatus], [CreatedByID], [UpdatedByID], [ModifiedDate]) VALUES (N'5', N'New Research Findings Shed Light on STEM', N'New Research Findings Shed Light on STEM', CAST(N'2024-05-05T00:00:00.000' AS DateTime), N'Groundbreaking research conducted by the Research Department of FU has unveiled significant findings in the field of STEM, offering fresh insights that could revolutionize current understanding and practices.', N'N/A', 1, 1, 2, 2, CAST(N'2024-05-05T00:00:00.000' AS DateTime))
GO

-- 5. SEED DATA FOR NEWSTAG
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'1', 5)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'1', 7)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'1', 9)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'2', 5)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'2', 7)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'2', 8)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'2', 9)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'3', 1)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'3', 8)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'3', 9)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'4', 1)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'4', 4)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'4', 8)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'4', 9)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'5', 2)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'5', 3)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'5', 4)
INSERT [dbo].[NewsTag] ([NewsArticleID], [TagID]) VALUES (N'5', 6)
GO

-- 6. AUTO-GENERATE 50 NEWS ARTICLES FOR TESTING (From Datatest.sql)
DECLARE @i INT = 6 
DECLARE @MaxPosts INT = 56 

WHILE @i <= @MaxPosts
BEGIN
    DECLARE @RandomCategoryID SMALLINT = (ABS(CHECKSUM(NEWID())) % 5) + 1 
    DECLARE @RandomAuthorID SMALLINT = (ABS(CHECKSUM(NEWID())) % 10) + 1 
    DECLARE @RandomDate DATETIME = DATEADD(DAY, - (ABS(CHECKSUM(NEWID())) % 365), GETDATE()) 
    DECLARE @RandomStatus BIT = CASE WHEN (ABS(CHECKSUM(NEWID())) % 10) > 2 THEN 1 ELSE 0 END 
    
    DECLARE @RandomViewCount INT = ABS(CHECKSUM(NEWID())) % 1000 
    
    DECLARE @Title NVARCHAR(400) = N'Auto Generated News Title Number ' + CAST(@i AS NVARCHAR(10)) + N' - Topic about ' + CASE @RandomCategoryID WHEN 1 THEN 'Academic' WHEN 2 THEN 'Student' ELSE 'General' END
    DECLARE @Headline NVARCHAR(150) = N'Breaking news headline for article ' + CAST(@i AS NVARCHAR(10))
    DECLARE @Content NVARCHAR(4000) = N'This is a generated content for testing purposes. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Article ID: ' + CAST(@i AS NVARCHAR(10))
    
    INSERT INTO [dbo].[NewsArticle] 
    ([NewsArticleID], [NewsTitle], [Headline], [CreatedDate], [NewsContent], [NewsSource], [CategoryID], [NewsStatus], [CreatedByID], [UpdatedByID], [ModifiedDate], [ViewCount]) 
    VALUES 
    (CAST(@i AS NVARCHAR(20)), @Title, @Headline, @RandomDate, @Content, N'FPT University Internal', @RandomCategoryID, @RandomStatus, @RandomAuthorID, @RandomAuthorID, @RandomDate, @RandomViewCount)

    -- Insert ngẫu nhiên Tags cho bài viết vừa tạo
    INSERT INTO [dbo].[NewsTag] ([NewsArticleID], [TagID])
    VALUES (CAST(@i AS NVARCHAR(20)), (ABS(CHECKSUM(NEWID())) % 15) + 1)
    
    BEGIN TRY
        INSERT INTO [dbo].[NewsTag] ([NewsArticleID], [TagID])
        VALUES (CAST(@i AS NVARCHAR(20)), (ABS(CHECKSUM(NEWID())) % 15) + 1)
    END TRY
    BEGIN CATCH
    END CATCH

    SET @i = @i + 1
END
GO

-- 7. SEED DATA FOR AUDITLOG
INSERT [dbo].[AuditLog] ([UserId], [UserName], [UserEmail], [Action], [EntityName], [EntityId], [OldValues], [NewValues], [Timestamp]) 
VALUES (1, N'Emma William', N'EmmaWilliam@FUNewsManagement.org', N'Create', N'NewsArticle', N'1', NULL, N'{"Title":"University FU Celebrates Success...","Status":1}', DATEADD(DAY, -10, GETDATE()))
INSERT [dbo].[AuditLog] ([UserId], [UserName], [UserEmail], [Action], [EntityName], [EntityId], [OldValues], [NewValues], [Timestamp]) 
VALUES (2, N'Olivia James', N'OliviaJames@FUNewsManagement.org', N'Update', N'NewsArticle', N'2', N'{"Status":0}', N'{"Status":1}', DATEADD(DAY, -5, GETDATE()))
INSERT [dbo].[AuditLog] ([UserId], [UserName], [UserEmail], [Action], [EntityName], [EntityId], [OldValues], [NewValues], [Timestamp]) 
VALUES (3, N'Isabella David', N'IsabellaDavid@FUNewsManagement.org', N'Create', N'Category', N'6', NULL, N'{"Name":"International Cooperation"}', DATEADD(DAY, -2, GETDATE()))
INSERT [dbo].[AuditLog] ([UserId], [UserName], [UserEmail], [Action], [EntityName], [EntityId], [OldValues], [NewValues], [Timestamp]) 
VALUES (1, N'Emma William', N'EmmaWilliam@FUNewsManagement.org', N'Delete', N'Tag', N'99', N'{"TagName":"Legacy"}', NULL, DATEADD(HOUR, -5, GETDATE()))
INSERT [dbo].[AuditLog] ([UserId], [UserName], [UserEmail], [Action], [EntityName], [EntityId], [OldValues], [NewValues], [Timestamp]) 
VALUES (4, N'Michael Charlotte', N'MichaelCharlotte@FUNewsManagement.org', N'Update', N'NewsArticle', N'5', N'{"ViewCount":10}', N'{"ViewCount":150}', GETDATE())
GO
