-- Update: 005_developer_info_is_verified.sql
-- Add is_verified column to DeveloperInfo table for tracking verification status

ALTER TABLE DeveloperInfo ADD COLUMN IF NOT EXISTS is_verified BOOLEAN DEFAULT false;
