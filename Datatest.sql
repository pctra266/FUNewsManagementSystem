USE [FUNewsManagement]
GO

-- 1. THÊM DỮ LIỆU SYSTEM ACCOUNT (Thêm 5 người dùng mới)
INSERT [dbo].[SystemAccount] ([AccountID], [AccountName], [AccountEmail], [AccountRole], [AccountPassword]) 
VALUES 
(6, N'John Editor', N'john.editor@funews.org', 1, N'@1'),
(7, N'Sarah Staff', N'sarah.staff@funews.org', 2, N'@1'),
(8, N'Robert Lecturer', N'robert.lec@funews.org', 2, N'@1'),
(9, N'Admin Root', N'admin@funews.org', 0, N'@1'), -- Role 0 cho Admin cấp cao
(10, N'Guest Writer', N'guest@funews.org', 2, N'@1')
GO

-- 2. THÊM DỮ LIỆU TAG (Thêm các chủ đề hot)
INSERT [dbo].[Tag] ([TagID], [TagName], [Note]) 
VALUES 
(10, N'Scholarship', N'Scholarship Opportunities'),
(11, N'Start-up', N'Student Start-ups'),
(12, N'Sports', N'Football and physical activities'),
(13, N'Examination', N'Mid-term and Final Exams'),
(14, N'Career', N'Job opportunities'),
(15, N'Clubs', N'FPTU Clubs')
GO

-- 3. TỰ ĐỘNG SINH 50 BÀI VIẾT (NEWS ARTICLE) VÀ GẮN TAG NGẪU NHIÊN
-- Sử dụng vòng lặp để tạo dữ liệu giả lập
DECLARE @i INT = 6 -- Bắt đầu từ ID 6 (vì bạn đã có 5 bài đầu)
DECLARE @MaxPosts INT = 56 -- Tạo đến bài số 56 (tức là thêm 50 bài)

WHILE @i <= @MaxPosts
BEGIN
    -- Các biến ngẫu nhiên
    DECLARE @RandomCategoryID SMALLINT = (ABS(CHECKSUM(NEWID())) % 5) + 1 -- Random Category từ 1 đến 5
    DECLARE @RandomAuthorID SMALLINT = (ABS(CHECKSUM(NEWID())) % 10) + 1 -- Random Author từ 1 đến 10
    DECLARE @RandomDate DATETIME = DATEADD(DAY, - (ABS(CHECKSUM(NEWID())) % 365), GETDATE()) -- Random ngày trong 1 năm qua
    DECLARE @RandomStatus BIT = CASE WHEN (ABS(CHECKSUM(NEWID())) % 10) > 2 THEN 1 ELSE 0 END -- 80% là Active (1), 20% là Inactive (0)
    
    -- Tạo tiêu đề và nội dung giả
    DECLARE @Title NVARCHAR(400) = N'Auto Generated News Title Number ' + CAST(@i AS NVARCHAR(10)) + N' - Topic about ' + CASE @RandomCategoryID WHEN 1 THEN 'Academic' WHEN 2 THEN 'Student' ELSE 'General' END
    DECLARE @Headline NVARCHAR(150) = N'Breaking news headline for article ' + CAST(@i AS NVARCHAR(10))
    DECLARE @Content NVARCHAR(4000) = N'This is a generated content for testing purposes. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Article ID: ' + CAST(@i AS NVARCHAR(10))
    
    -- Insert vào NewsArticle
    INSERT INTO [dbo].[NewsArticle] 
    ([NewsArticleID], [NewsTitle], [Headline], [CreatedDate], [NewsContent], [NewsSource], [CategoryID], [NewsStatus], [CreatedByID], [UpdatedByID], [ModifiedDate]) 
    VALUES 
    (
        CAST(@i AS NVARCHAR(20)), -- ID dạng chuỗi
        @Title, 
        @Headline, 
        @RandomDate, 
        @Content, 
        N'FPT University Internal', 
        @RandomCategoryID, 
        @RandomStatus, 
        @RandomAuthorID, 
        @RandomAuthorID, 
        @RandomDate
    )

    -- Insert ngẫu nhiên Tags cho bài viết vừa tạo (Mỗi bài 2 tag)
    -- Tag 1
    INSERT INTO [dbo].[NewsTag] ([NewsArticleID], [TagID])
    VALUES (CAST(@i AS NVARCHAR(20)), (ABS(CHECKSUM(NEWID())) % 15) + 1)
    
    -- Tag 2 (Cố gắng chọn tag khác tag 1, nếu trùng thì bỏ qua nhờ Primary Key ignore hoặc try catch, ở đây ta cứ insert đơn giản)
    BEGIN TRY
        INSERT INTO [dbo].[NewsTag] ([NewsArticleID], [TagID])
        VALUES (CAST(@i AS NVARCHAR(20)), (ABS(CHECKSUM(NEWID())) % 15) + 1)
    END TRY
    BEGIN CATCH
        -- Bỏ qua nếu trùng TagID với bài viết đó
    END CATCH

    SET @i = @i + 1
END
GO