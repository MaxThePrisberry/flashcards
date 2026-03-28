CREATE TABLE "Likes" (
    like_id    UUID PRIMARY KEY,
    user_id    UUID NOT NULL REFERENCES "User"(user_id) ON DELETE CASCADE,
    deck_id    UUID NOT NULL REFERENCES "Deck"(deck_id) ON DELETE CASCADE,
);
