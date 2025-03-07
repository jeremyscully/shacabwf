-- SQL Script to create test users with proper roles
-- First, ensure the Roles column exists
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Roles'
)
BEGIN
    -- Add Roles column to Users table
    ALTER TABLE Users
    ADD Roles NVARCHAR(255) NULL;
    
    PRINT 'Roles column added to Users table.';
END
ELSE
BEGIN
    PRINT 'Roles column already exists in Users table.';
END

-- Create or update admin user
IF EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    UPDATE Users
    SET 
        Email = 'admin@example.com',
        Password = 'Admin123!',
        FirstName = 'Admin',
        LastName = 'User',
        Department = 'IT',
        IsCABMember = 1,
        IsSupportPersonnel = 1,
        Roles = 'User,Admin,CABMember,Support'
    WHERE Username = 'admin';
    
    PRINT 'Admin user updated.';
END
ELSE
BEGIN
    INSERT INTO Users (Username, Email, Password, FirstName, LastName, Department, IsCABMember, IsSupportPersonnel, Roles)
    VALUES ('admin', 'admin@example.com', 'Admin123!', 'Admin', 'User', 'IT', 1, 1, 'User,Admin,CABMember,Support');
    
    PRINT 'Admin user created.';
END

-- Create or update manager user
IF EXISTS (SELECT * FROM Users WHERE Username = 'manager')
BEGIN
    UPDATE Users
    SET 
        Email = 'manager@example.com',
        Password = 'Manager123!',
        FirstName = 'Manager',
        LastName = 'User',
        Department = 'Operations',
        IsCABMember = 1,
        IsSupportPersonnel = 0,
        Roles = 'User,CABMember'
    WHERE Username = 'manager';
    
    PRINT 'Manager user updated.';
END
ELSE
BEGIN
    INSERT INTO Users (Username, Email, Password, FirstName, LastName, Department, IsCABMember, IsSupportPersonnel, Roles)
    VALUES ('manager', 'manager@example.com', 'Manager123!', 'Manager', 'User', 'Operations', 1, 0, 'User,CABMember');
    
    PRINT 'Manager user created.';
END

-- Create or update regular user
DECLARE @ManagerId INT;
SELECT @ManagerId = Id FROM Users WHERE Username = 'manager';

IF EXISTS (SELECT * FROM Users WHERE Username = 'user1')
BEGIN
    UPDATE Users
    SET 
        Email = 'user1@example.com',
        Password = 'User123!',
        FirstName = 'Regular',
        LastName = 'User',
        Department = 'Sales',
        IsCABMember = 0,
        IsSupportPersonnel = 0,
        SupervisorId = @ManagerId,
        Roles = 'User'
    WHERE Username = 'user1';
    
    PRINT 'Regular user updated.';
END
ELSE
BEGIN
    INSERT INTO Users (Username, Email, Password, FirstName, LastName, Department, IsCABMember, IsSupportPersonnel, SupervisorId, Roles)
    VALUES ('user1', 'user1@example.com', 'User123!', 'Regular', 'User', 'Sales', 0, 0, @ManagerId, 'User');
    
    PRINT 'Regular user created.';
END

-- Create or update support user
IF EXISTS (SELECT * FROM Users WHERE Username = 'support')
BEGIN
    UPDATE Users
    SET 
        Email = 'support@example.com',
        Password = 'Support123!',
        FirstName = 'Support',
        LastName = 'User',
        Department = 'IT Support',
        IsCABMember = 0,
        IsSupportPersonnel = 1,
        Roles = 'User,Support'
    WHERE Username = 'support';
    
    PRINT 'Support user updated.';
END
ELSE
BEGIN
    INSERT INTO Users (Username, Email, Password, FirstName, LastName, Department, IsCABMember, IsSupportPersonnel, Roles)
    VALUES ('support', 'support@example.com', 'Support123!', 'Support', 'User', 'IT Support', 0, 1, 'User,Support');
    
    PRINT 'Support user created.';
END

PRINT 'Test users creation/update completed successfully.'; 