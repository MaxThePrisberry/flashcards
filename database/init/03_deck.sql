CREATE TABLE "Deck" (
    deck_id     SERIAL PRIMARY KEY,
    user_id     INT          NOT NULL REFERENCES "User"(user_id) ON DELETE CASCADE,
    title       VARCHAR(255) NOT NULL,
    description TEXT,
    is_public   BOOLEAN      NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMP    NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMP    NOT NULL DEFAULT NOW()
);
