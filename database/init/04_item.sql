CREATE TABLE "Item" (
    item_id    SERIAL PRIMARY KEY,
    deck_id    INT  NOT NULL REFERENCES "Deck"(deck_id) ON DELETE CASCADE,
    type_id    INT  NOT NULL REFERENCES "Type"(type_id),
    value      TEXT NOT NULL,
    position   INT  NOT NULL DEFAULT 0
);
