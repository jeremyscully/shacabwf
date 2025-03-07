-- SQL Script to generate test change requests for user1
-- This script assumes the following:
-- 1. User with ID 3 is user1 (as per our seed data)
-- 2. The ChangeRequests table exists with the appropriate schema

-- Clear existing change requests for user1 (optional, comment out if you want to keep existing data)
DELETE FROM ChangeRequestHistory WHERE ChangeRequestId IN (SELECT Id FROM ChangeRequests WHERE CreatedById = 3);
DELETE FROM ChangeRequestComments WHERE ChangeRequestId IN (SELECT Id FROM ChangeRequests WHERE CreatedById = 3);
DELETE FROM ChangeRequestAssignments WHERE ChangeRequestId IN (SELECT Id FROM ChangeRequests WHERE CreatedById = 3);
DELETE FROM ChangeRequestApprovals WHERE ChangeRequestId IN (SELECT Id FROM ChangeRequests WHERE CreatedById = 3);
DELETE FROM ChangeRequests WHERE CreatedById = 3;

-- Variables for date calculations
DECLARE @Today DATE = GETDATE();
DECLARE @OneMonthFromNow DATE = DATEADD(MONTH, 1, @Today);
DECLARE @TwoWeeksFromNow DATE = DATEADD(WEEK, 2, @Today);
DECLARE @OneWeekFromNow DATE = DATEADD(WEEK, 1, @Today);
DECLARE @ThreeDaysFromNow DATE = DATEADD(DAY, 3, @Today);
DECLARE @Yesterday DATE = DATEADD(DAY, -1, @Today);
DECLARE @LastWeek DATE = DATEADD(WEEK, -1, @Today);

-- Insert change requests with different statuses, priorities, and dates

-- 1. Draft change request (created today)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00001',
    'Update Server Configuration',
    'Update the configuration settings on the application server to improve performance.',
    'Current configuration is causing performance issues during peak hours.',
    'Low risk as changes can be reverted if issues arise.',
    'Revert to previous configuration settings if performance degrades.',
    0, -- Draft
    1, -- Medium
    0, -- Normal
    1, -- Medium
    @Today,
    3 -- user1
);

-- 2. Submitted for supervisor approval (created 3 days ago)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00002',
    'Database Index Optimization',
    'Add new indexes to improve query performance on the customer database.',
    'Queries are currently taking too long to execute, affecting user experience.',
    'Medium risk as new indexes may affect write performance.',
    'Remove the new indexes and revert to previous configuration.',
    1, -- SubmittedForSupervisorApproval
    2, -- High
    0, -- Normal
    1, -- Medium
    @LastWeek,
    @ThreeDaysFromNow,
    3 -- user1
);

-- 3. Supervisor approved (created last week, scheduled for next week)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, ScheduledStartDate, ScheduledEndDate, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00003',
    'Security Patch Installation',
    'Install the latest security patches on all production servers.',
    'Critical security vulnerabilities need to be addressed immediately.',
    'Medium risk as patches may cause compatibility issues with existing software.',
    'Uninstall patches and restore from backup if issues arise.',
    2, -- SupervisorApproved
    3, -- Critical
    0, -- Normal
    2, -- High
    @LastWeek,
    @Yesterday,
    @OneWeekFromNow,
    @OneWeekFromNow,
    3 -- user1
);

-- 4. CAB Approved (created 2 weeks ago, scheduled for tomorrow)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, ScheduledStartDate, ScheduledEndDate, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00004',
    'Network Firewall Update',
    'Update firewall rules to enhance security and block recent attack vectors.',
    'Recent security audit identified potential vulnerabilities in our network.',
    'Medium risk as rule changes may affect legitimate traffic.',
    'Revert to previous firewall configuration if issues arise.',
    5, -- CABApproved
    2, -- High
    0, -- Normal
    2, -- High
    DATEADD(WEEK, -2, @Today),
    @Yesterday,
    DATEADD(DAY, 1, @Today),
    DATEADD(DAY, 1, @Today),
    3 -- user1
);

-- 5. Scheduled (created 3 weeks ago, scheduled for next week)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, ScheduledStartDate, ScheduledEndDate, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00005',
    'Application Server Upgrade',
    'Upgrade application servers to the latest version.',
    'Current version will reach end-of-life next quarter.',
    'High risk as this is a major version upgrade.',
    'Rollback to previous version using system backups.',
    7, -- Scheduled
    1, -- Medium
    0, -- Normal
    1, -- Medium
    DATEADD(WEEK, -3, @Today),
    @Yesterday,
    @OneWeekFromNow,
    DATEADD(DAY, 8, @Today),
    3 -- user1
);

-- 6. In Progress (created 2 weeks ago, started yesterday)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, ScheduledStartDate, ScheduledEndDate, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00006',
    'Storage Capacity Expansion',
    'Add additional storage capacity to the data warehouse.',
    'Current storage is at 85% capacity and needs to be expanded.',
    'Low risk as this is an additive change.',
    'No backout plan needed as this is an expansion.',
    8, -- InProgress
    1, -- Medium
    0, -- Normal
    0, -- Low
    DATEADD(WEEK, -2, @Today),
    @Yesterday,
    @Yesterday,
    @ThreeDaysFromNow,
    3 -- user1
);

-- 7. Completed (created 3 weeks ago, completed yesterday)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, ScheduledStartDate, ScheduledEndDate, ImplementedAt, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00007',
    'Email Server Configuration',
    'Update email server configuration to improve deliverability.',
    'Some emails are being marked as spam by recipient servers.',
    'Low risk as changes are incremental and can be reverted.',
    'Revert to previous configuration if deliverability worsens.',
    9, -- Completed
    0, -- Low
    0, -- Normal
    0, -- Low
    DATEADD(WEEK, -3, @Today),
    @Yesterday,
    DATEADD(DAY, -3, @Today),
    DATEADD(DAY, -2, @Today),
    @Yesterday,
    3 -- user1
);

-- 8. Failed (created 2 weeks ago, failed yesterday)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, ScheduledStartDate, ScheduledEndDate, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00008',
    'Database Migration',
    'Migrate database to new cloud platform.',
    'Current database platform is being deprecated.',
    'High risk due to complexity of migration.',
    'Revert to original database if migration fails.',
    10, -- Failed
    2, -- High
    0, -- Normal
    2, -- High
    DATEADD(WEEK, -2, @Today),
    @Yesterday,
    DATEADD(DAY, -2, @Today),
    @Yesterday,
    3 -- user1
);

-- 9. Emergency change (created yesterday, scheduled for today)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, ScheduledStartDate, ScheduledEndDate, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00009',
    'Critical Security Hotfix',
    'Apply emergency security hotfix to address zero-day vulnerability.',
    'Systems are vulnerable to active exploit in the wild.',
    'Medium risk but necessary due to active threats.',
    'Revert patch if it causes critical system issues.',
    5, -- CABApproved
    3, -- Critical
    2, -- Emergency
    2, -- High
    @Yesterday,
    @Today,
    @Today,
    @Today,
    3 -- user1
);

-- 10. Future change (created today, scheduled for one month from now)
INSERT INTO ChangeRequests (
    ChangeRequestNumber, Title, Description, Justification, RiskAssessment, BackoutPlan,
    Status, Priority, Type, Impact, CreatedAt, UpdatedAt, ScheduledStartDate, ScheduledEndDate, CreatedById
)
VALUES (
    'CR-' + FORMAT(@Today, 'yyyy') + '-00010',
    'Annual System Maintenance',
    'Perform annual system maintenance and upgrades.',
    'Regular maintenance required to keep systems running optimally.',
    'Medium risk due to multiple systems being affected.',
    'Each component has its own backout plan documented in the maintenance guide.',
    7, -- Scheduled
    1, -- Medium
    0, -- Normal
    1, -- Medium
    @Today,
    @Today,
    @OneMonthFromNow,
    DATEADD(DAY, 2, @OneMonthFromNow),
    3 -- user1
);

-- Add some approvals
-- Supervisor approval for CR-00003
INSERT INTO ChangeRequestApprovals (
    ChangeRequestId, ApproverId, Type, Status, Comments, RequestedAt, ActionedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00003'),
    2, -- manager (supervisor)
    0, -- Supervisor
    1, -- Approved
    'Approved. Please proceed with caution.',
    @LastWeek,
    @Yesterday
);

-- CAB approval for CR-00004
INSERT INTO ChangeRequestApprovals (
    ChangeRequestId, ApproverId, Type, Status, Comments, RequestedAt, ActionedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00004'),
    1, -- admin (CAB member)
    1, -- CAB
    1, -- Approved
    'Approved by CAB. Schedule during maintenance window.',
    DATEADD(DAY, -3, @Today),
    @Yesterday
);

-- Add some assignments
-- Assign CR-00005 to support
INSERT INTO ChangeRequestAssignments (
    ChangeRequestId, AssigneeId, Role, Notes, AssignedAt, Status
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00005'),
    4, -- support
    'Implementer',
    'Please implement according to the change plan.',
    @Yesterday,
    0 -- Assigned
);

-- Assign CR-00006 to support
INSERT INTO ChangeRequestAssignments (
    ChangeRequestId, AssigneeId, Role, Notes, AssignedAt, Status
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00006'),
    4, -- support
    'Implementer',
    'Please implement according to the change plan.',
    DATEADD(DAY, -2, @Today),
    1 -- InProgress
);

-- Add some comments
-- Comment on CR-00003
INSERT INTO ChangeRequestComments (
    ChangeRequestId, CommenterId, Text, IsInternal, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00003'),
    3, -- user1
    'Added additional details to the implementation plan.',
    0, -- Not internal
    DATEADD(DAY, -3, @Today)
);

-- Comment on CR-00004
INSERT INTO ChangeRequestComments (
    ChangeRequestId, CommenterId, Text, IsInternal, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00004'),
    2, -- manager
    'Please ensure all stakeholders are notified before implementation.',
    0, -- Not internal
    DATEADD(DAY, -2, @Today)
);

-- Internal comment on CR-00004
INSERT INTO ChangeRequestComments (
    ChangeRequestId, CommenterId, Text, IsInternal, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00004'),
    1, -- admin
    'This change needs careful monitoring during implementation.',
    1, -- Internal
    @Yesterday
);

-- Add some history entries
-- History for CR-00003
INSERT INTO ChangeRequestHistory (
    ChangeRequestId, UserId, ActionType, Description, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00003'),
    3, -- user1
    'Created',
    'Change request created',
    @LastWeek
);

INSERT INTO ChangeRequestHistory (
    ChangeRequestId, UserId, ActionType, Description, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00003'),
    3, -- user1
    'Submitted',
    'Submitted for supervisor approval',
    DATEADD(DAY, -4, @Today)
);

INSERT INTO ChangeRequestHistory (
    ChangeRequestId, UserId, ActionType, Description, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00003'),
    2, -- manager
    'Approved',
    'Approved by supervisor',
    @Yesterday
);

-- History for CR-00004
INSERT INTO ChangeRequestHistory (
    ChangeRequestId, UserId, ActionType, Description, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00004'),
    3, -- user1
    'Created',
    'Change request created',
    DATEADD(WEEK, -2, @Today)
);

INSERT INTO ChangeRequestHistory (
    ChangeRequestId, UserId, ActionType, Description, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00004'),
    3, -- user1
    'Submitted',
    'Submitted for supervisor approval',
    DATEADD(DAY, -5, @Today)
);

INSERT INTO ChangeRequestHistory (
    ChangeRequestId, UserId, ActionType, Description, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00004'),
    2, -- manager
    'Approved',
    'Approved by supervisor',
    DATEADD(DAY, -3, @Today)
);

INSERT INTO ChangeRequestHistory (
    ChangeRequestId, UserId, ActionType, Description, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00004'),
    3, -- user1
    'Submitted',
    'Submitted for CAB approval',
    DATEADD(DAY, -2, @Today)
);

INSERT INTO ChangeRequestHistory (
    ChangeRequestId, UserId, ActionType, Description, CreatedAt
)
VALUES (
    (SELECT Id FROM ChangeRequests WHERE ChangeRequestNumber = 'CR-' + FORMAT(@Today, 'yyyy') + '-00004'),
    1, -- admin
    'Approved',
    'Approved by CAB',
    @Yesterday
);

-- Print confirmation
SELECT 'Test data generation complete. 10 change requests created for user1.' AS Message; 