CREATE TABLE "Type" (
    type_id    SERIAL PRIMARY KEY,
    type_name  VARCHAR(10) NOT NULL UNIQUE  -- 'text' or 'image'
);

-- Seed the two valid types immediately
INSERT INTO "Type" (type_name) VALUES ('text'), ('image');
