-- SQL Script to update user roles
-- This script updates the roles for existing users

-- Set default role for all users
UPDATE Users
SET Roles = 'User'
WHERE Roles IS NULL;

-- Update admin users
UPDATE Users
SET Roles = 'User,Admin,CABMember,Support'
WHERE Username = 'admin';

-- Update CAB members
UPDATE Users
SET Roles = 'User,CABMember'
WHERE IsCABMember = 1 
  AND Username != 'admin'
  AND (Roles IS NULL OR Roles = 'User');

-- Update support personnel
UPDATE Users
SET Roles = 'User,Support'
WHERE IsSupportPersonnel = 1 
  AND Username != 'admin'
  AND (Roles IS NULL OR Roles = 'User');

PRINT 'User roles updated successfully.'; 