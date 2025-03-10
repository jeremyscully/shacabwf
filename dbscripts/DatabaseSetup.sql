-- =============================================
-- Database Setup Script for ShacabWf
-- =============================================
-- This script will:
-- 1. Create the database if it doesn't exist
-- 2. Create all necessary tables
-- 3. Add required columns
-- 4. Seed initial data
-- =============================================

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'shacab')
BEGIN
    CREATE DATABASE shacab;
    PRINT 'Database created.';
END
ELSE
BEGIN
    PRINT 'Database already exists.';
END
GO

USE shacab;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Username] [nvarchar](100) NOT NULL,
        [Email] [nvarchar](100) NOT NULL,
        [Password] [nvarchar](100) NOT NULL,
        [FirstName] [nvarchar](100) NOT NULL,
        [LastName] [nvarchar](100) NOT NULL,
        [Department] [nvarchar](200) NOT NULL,
        [SupervisorId] [int] NULL,
        [IsCABMember] [bit] NOT NULL DEFAULT(0),
        [IsSupportPersonnel] [bit] NOT NULL DEFAULT(0),
        [Roles] [nvarchar](500) NOT NULL DEFAULT(''),
        [Theme] [nvarchar](50) NULL DEFAULT('Default'),
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Users_Users_SupervisorId] FOREIGN KEY([SupervisorId]) 
            REFERENCES [dbo].[Users] ([Id])
    );
    PRINT 'Users table created.';
END
ELSE
BEGIN
    -- Add Theme column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Theme')
    BEGIN
        ALTER TABLE Users ADD Theme NVARCHAR(50) NULL DEFAULT('Default');
        PRINT 'Theme column added to Users table.';
    END
    
    -- Add Roles column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Roles')
    BEGIN
        ALTER TABLE Users ADD Roles NVARCHAR(500) NOT NULL DEFAULT('');
        PRINT 'Roles column added to Users table.';
    END
    
    -- Add Password column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Password')
    BEGIN
        ALTER TABLE Users ADD Password NVARCHAR(100) NOT NULL DEFAULT('');
        PRINT 'Password column added to Users table.';
    END
    
    PRINT 'Users table already exists. Columns checked and added if needed.';
END
GO

-- Create ChangeRequests table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChangeRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChangeRequests](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ChangeRequestNumber] [nvarchar](20) NOT NULL,
        [Title] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](max) NOT NULL,
        [Justification] [nvarchar](max) NOT NULL,
        [ImpactAssessment] [nvarchar](max) NOT NULL,
        [RiskAssessment] [nvarchar](max) NOT NULL,
        [BackoutPlan] [nvarchar](max) NOT NULL,
        [Status] [nvarchar](50) NOT NULL,
        [Priority] [nvarchar](20) NOT NULL,
        [ChangeType] [nvarchar](50) NOT NULL,
        [SubmittedDate] [datetime2](7) NOT NULL,
        [PlannedStartDate] [datetime2](7) NOT NULL,
        [PlannedEndDate] [datetime2](7) NOT NULL,
        [ActualStartDate] [datetime2](7) NULL,
        [ActualEndDate] [datetime2](7) NULL,
        [CreatedById] [int] NOT NULL,
        CONSTRAINT [PK_ChangeRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChangeRequests_Users_CreatedById] FOREIGN KEY([CreatedById]) 
            REFERENCES [dbo].[Users] ([Id])
    );
    PRINT 'ChangeRequests table created.';
END
ELSE
BEGIN
    PRINT 'ChangeRequests table already exists.';
END
GO

-- Create ChangeRequestApprovals table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChangeRequestApprovals]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChangeRequestApprovals](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ChangeRequestId] [int] NOT NULL,
        [ApproverId] [int] NOT NULL,
        [ApprovalStatus] [nvarchar](50) NOT NULL,
        [ApprovalDate] [datetime2](7) NULL,
        [Comments] [nvarchar](max) NULL,
        CONSTRAINT [PK_ChangeRequestApprovals] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChangeRequestApprovals_ChangeRequests_ChangeRequestId] FOREIGN KEY([ChangeRequestId]) 
            REFERENCES [dbo].[ChangeRequests] ([Id]),
        CONSTRAINT [FK_ChangeRequestApprovals_Users_ApproverId] FOREIGN KEY([ApproverId]) 
            REFERENCES [dbo].[Users] ([Id])
    );
    PRINT 'ChangeRequestApprovals table created.';
END
ELSE
BEGIN
    PRINT 'ChangeRequestApprovals table already exists.';
END
GO

-- Create ChangeRequestAssignments table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChangeRequestAssignments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChangeRequestAssignments](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ChangeRequestId] [int] NOT NULL,
        [AssigneeId] [int] NOT NULL,
        [AssignmentDate] [datetime2](7) NOT NULL,
        [AssignmentType] [nvarchar](50) NOT NULL,
        CONSTRAINT [PK_ChangeRequestAssignments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChangeRequestAssignments_ChangeRequests_ChangeRequestId] FOREIGN KEY([ChangeRequestId]) 
            REFERENCES [dbo].[ChangeRequests] ([Id]),
        CONSTRAINT [FK_ChangeRequestAssignments_Users_AssigneeId] FOREIGN KEY([AssigneeId]) 
            REFERENCES [dbo].[Users] ([Id])
    );
    PRINT 'ChangeRequestAssignments table created.';
END
ELSE
BEGIN
    PRINT 'ChangeRequestAssignments table already exists.';
END
GO

-- Create ChangeRequestComments table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChangeRequestComments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChangeRequestComments](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ChangeRequestId] [int] NOT NULL,
        [CommenterId] [int] NOT NULL,
        [CommentText] [nvarchar](max) NOT NULL,
        [CommentDate] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ChangeRequestComments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChangeRequestComments_ChangeRequests_ChangeRequestId] FOREIGN KEY([ChangeRequestId]) 
            REFERENCES [dbo].[ChangeRequests] ([Id]),
        CONSTRAINT [FK_ChangeRequestComments_Users_CommenterId] FOREIGN KEY([CommenterId]) 
            REFERENCES [dbo].[Users] ([Id])
    );
    PRINT 'ChangeRequestComments table created.';
END
ELSE
BEGIN
    PRINT 'ChangeRequestComments table already exists.';
END
GO

-- Create ChangeRequestHistory table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChangeRequestHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChangeRequestHistory](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ChangeRequestId] [int] NOT NULL,
        [UserId] [int] NOT NULL,
        [ChangeDate] [datetime2](7) NOT NULL,
        [FieldName] [nvarchar](100) NOT NULL,
        [OldValue] [nvarchar](max) NULL,
        [NewValue] [nvarchar](max) NULL,
        CONSTRAINT [PK_ChangeRequestHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChangeRequestHistory_ChangeRequests_ChangeRequestId] FOREIGN KEY([ChangeRequestId]) 
            REFERENCES [dbo].[ChangeRequests] ([Id]),
        CONSTRAINT [FK_ChangeRequestHistory_Users_UserId] FOREIGN KEY([UserId]) 
            REFERENCES [dbo].[Users] ([Id])
    );
    PRINT 'ChangeRequestHistory table created.';
END
ELSE
BEGIN
    PRINT 'ChangeRequestHistory table already exists.';
END
GO

-- Seed initial users
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
        Roles = 'User,Admin,CABMember,Support',
        Theme = 'Default'
    WHERE Username = 'admin';
    
    PRINT 'Admin user updated.';
END
ELSE
BEGIN
    INSERT INTO Users (Username, Email, Password, FirstName, LastName, Department, IsCABMember, IsSupportPersonnel, Roles, Theme)
    VALUES ('admin', 'admin@example.com', 'Admin123!', 'Admin', 'User', 'IT', 1, 1, 'User,Admin,CABMember,Support', 'Default');
    
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
        Roles = 'User,CABMember',
        Theme = 'Default'
    WHERE Username = 'manager';
    
    PRINT 'Manager user updated.';
END
ELSE
BEGIN
    INSERT INTO Users (Username, Email, Password, FirstName, LastName, Department, IsCABMember, IsSupportPersonnel, Roles, Theme)
    VALUES ('manager', 'manager@example.com', 'Manager123!', 'Manager', 'User', 'Operations', 1, 0, 'User,CABMember', 'Default');
    
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
        Roles = 'User',
        Theme = 'Default'
    WHERE Username = 'user1';
    
    PRINT 'Regular user updated.';
END
ELSE
BEGIN
    INSERT INTO Users (Username, Email, Password, FirstName, LastName, Department, IsCABMember, IsSupportPersonnel, SupervisorId, Roles, Theme)
    VALUES ('user1', 'user1@example.com', 'User123!', 'Regular', 'User', 'Sales', 0, 0, @ManagerId, 'User', 'Default');
    
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
        Roles = 'User,Support',
        Theme = 'Default'
    WHERE Username = 'support';
    
    PRINT 'Support user updated.';
END
ELSE
BEGIN
    INSERT INTO Users (Username, Email, Password, FirstName, LastName, Department, IsCABMember, IsSupportPersonnel, Roles, Theme)
    VALUES ('support', 'support@example.com', 'Support123!', 'Support', 'User', 'IT Support', 0, 1, 'User,Support', 'Default');
    
    PRINT 'Support user created.';
END

-- Create sample change requests if none exist
IF NOT EXISTS (SELECT TOP 1 * FROM ChangeRequests)
BEGIN
    -- Get user IDs
    DECLARE @AdminId INT, @UserId INT;
    SELECT @AdminId = Id FROM Users WHERE Username = 'admin';
    SELECT @UserId = Id FROM Users WHERE Username = 'user1';
    
    -- Insert a sample change request
    INSERT INTO ChangeRequests (
        ChangeRequestNumber, 
        Title, 
        Description, 
        Justification, 
        ImpactAssessment, 
        RiskAssessment, 
        BackoutPlan, 
        Status, 
        Priority, 
        ChangeType, 
        SubmittedDate, 
        PlannedStartDate, 
        PlannedEndDate, 
        CreatedById
    )
    VALUES (
        'CR-2023-001',
        'Server Upgrade',
        'Upgrade the production server from Windows Server 2016 to Windows Server 2022',
        'Current server OS is approaching end of support. Upgrade needed for security and performance improvements.',
        'Minimal impact expected. Scheduled during off-hours.',
        'Medium risk. Potential for application compatibility issues.',
        'Restore from backup if upgrade fails.',
        'Submitted',
        'Medium',
        'Infrastructure',
        GETDATE(),
        DATEADD(DAY, 7, GETDATE()),
        DATEADD(DAY, 7, GETDATE()),
        @UserId
    );
    
    DECLARE @ChangeRequestId INT = SCOPE_IDENTITY();
    
    -- Add an approval
    INSERT INTO ChangeRequestApprovals (
        ChangeRequestId,
        ApproverId,
        ApprovalStatus,
        ApprovalDate,
        Comments
    )
    VALUES (
        @ChangeRequestId,
        @AdminId,
        'Pending',
        NULL,
        NULL
    );
    
    -- Add an assignment
    INSERT INTO ChangeRequestAssignments (
        ChangeRequestId,
        AssigneeId,
        AssignmentDate,
        AssignmentType
    )
    VALUES (
        @ChangeRequestId,
        @AdminId,
        GETDATE(),
        'Reviewer'
    );
    
    -- Add a comment
    INSERT INTO ChangeRequestComments (
        ChangeRequestId,
        CommenterId,
        CommentText,
        CommentDate
    )
    VALUES (
        @ChangeRequestId,
        @UserId,
        'Initial submission for review.',
        GETDATE()
    );
    
    PRINT 'Sample change request created.';
END
ELSE
BEGIN
    PRINT 'Change requests already exist. Skipping sample data creation.';
END

PRINT 'Database setup completed successfully.';
GO 