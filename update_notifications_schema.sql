-- Update database schema for Notifications table

-- Step 1: Add the Type column to the Notifications table
ALTER TABLE Notifications ADD Type INT NOT NULL DEFAULT 0;

-- Step 2: Remove the Url column from the Notifications table  
ALTER TABLE Notifications DROP COLUMN Url;

-- Step 3: Update the __EFMigrationsHistory table to mark the migration as applied
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('20251229050000_AddNotificationTypeColumn', '8.0.8');

PRINT 'Database schema updated successfully!';
PRINT '- Added Type column to Notifications table';
PRINT '- Removed Url column from Notifications table';
PRINT '- Marked migration as applied in history';