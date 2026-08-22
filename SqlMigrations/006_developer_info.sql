-- Migration: 006_developer_info.sql
-- Developer / company info persisted when a scan has no public market data
-- (Panel 3 "Developer info" path). Market data scans go to stock_data instead.

CREATE SEQUENCE IF NOT EXISTS seq_developer_info_id START 1;

CREATE TABLE IF NOT EXISTS DeveloperInfo (
    id INTEGER PRIMARY KEY DEFAULT nextval('seq_developer_info_id'),
    user_id INTEGER NOT NULL DEFAULT 1,
    ai_vision_icon_detail_id INTEGER,
    app_name VARCHAR,
    company VARCHAR,
    website VARCHAR,
    download_url VARCHAR,
    github VARCHAR,
    linkedin VARCHAR,
    twitter VARCHAR,
    email VARCHAR,
    description TEXT,
    raw_info_text TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_developer_info_user_id ON DeveloperInfo(user_id);
CREATE INDEX IF NOT EXISTS idx_developer_info_app_name ON DeveloperInfo(app_name);
CREATE INDEX IF NOT EXISTS idx_developer_info_ai_vision_icon_detail_id ON DeveloperInfo(ai_vision_icon_detail_id);