-- Add Risk column to ChangeRequests table
ALTER TABLE ChangeRequests ADD Risk INT NOT NULL DEFAULT 0; 