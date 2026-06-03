-- Migration: 002_user_profiles.sql
-- Create a separate user_profiles table linked to users

CREATE SEQUENCE IF NOT EXISTS seq_user_profiles_id START 1;

CREATE TABLE IF NOT EXISTS user_profiles (
    id INTEGER PRIMARY KEY DEFAULT nextval('seq_user_profiles_id'),
    user_id INTEGER NOT NULL,
    display_name VARCHAR,
    bio TEXT,
    avatar_url VARCHAR,
    preferences TEXT,
    is_public BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_user_profiles_user_id ON user_profiles(user_id);
