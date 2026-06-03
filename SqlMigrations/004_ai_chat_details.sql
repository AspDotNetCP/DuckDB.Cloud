-- Migration: 004_ai_chat_details.sql
-- Stores AI chat response results

CREATE TABLE IF NOT EXISTS AiChatDetails (
    id INTEGER PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    provider VARCHAR NOT NULL,
    original_prompt TEXT,
    raw_response TEXT,
    success BOOLEAN NOT NULL DEFAULT FALSE,
    elapsed_ms INTEGER,
    error_message TEXT,
    is_rate_limit BOOLEAN NOT NULL DEFAULT FALSE,
    is_quota BOOLEAN NOT NULL DEFAULT FALSE,
    is_unavailable BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
