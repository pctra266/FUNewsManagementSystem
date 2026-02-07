
ALTER TABLE [dbo].[SystemAccount]
ADD [RefreshToken] NVARCHAR(200) NULL,
    [RefreshTokenExpiryTime] DATETIME NULL;