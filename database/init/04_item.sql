CREATE TABLE "Item" (
    item_id    UUID PRIMARY KEY,
    deck_id    UUID  NOT NULL REFERENCES "Deck"(deck_id) ON DELETE CASCADE,
    type_id    UUID  NOT NULL REFERENCES "Type"(type_id),
    value      TEXT NOT NULL,
    position   INT  NOT NULL DEFAULT 0
);
