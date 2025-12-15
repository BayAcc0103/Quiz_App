-- SQL Script to create RecommendedQuizzes table
-- This script can be executed directly against your SQL Server database

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RecommendedQuizzes' AND xtype='U')
BEGIN
    CREATE TABLE RecommendedQuizzes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        QuizId NVARCHAR(MAX) NOT NULL,  -- Using NVARCHAR(MAX) to accommodate GUID strings
        PredictedRating DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );

    -- Create index for better performance when querying by UserId
    CREATE INDEX IX_RecommendedQuizzes_UserId ON RecommendedQuizzes(UserId);

    PRINT 'RecommendedQuizzes table created successfully';
END
ELSE
BEGIN
    PRINT 'RecommendedQuizzes table already exists';
END