-- SQL Script to add Roles column to Users table
-- This is a simplified version that only adds the column

-- Add Roles column to Users table if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Roles'
)
BEGIN
    ALTER TABLE Users
    ADD Roles NVARCHAR(255) NULL;
    
    PRINT 'Roles column added to Users table.';
END
ELSE
BEGIN
    PRINT 'Roles column already exists in Users table.';
END 