-- Update: add scanner user to existing AiVisionIconDetails deployments
ALTER TABLE AiVisionIconDetails ADD COLUMN IF NOT EXISTS user_id INTEGER;

UPDATE AiVisionIconDetails
SET user_id = 1
WHERE user_id IS NULL;

CREATE INDEX IF NOT EXISTS idx_ai_vision_icon_details_user_id ON AiVisionIconDetails(user_id);
