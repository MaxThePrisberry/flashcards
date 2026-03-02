const API_URL = process.env.NEXT_PUBLIC_API_URL;

function authHeaders() {
  const token = localStorage.getItem("token");

  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
}

// CREATE DECK
export async function createDeck(data: {
  title: string;
  description?: string;
  cards: { term: string; definition: string }[];
}) {
  const res = await fetch(`${API_URL}/api/decks`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    throw new Error("Failed to create deck");
  }

  return res.json();
}

// GET ALL DECKS
export async function getDecks() {
  const res = await fetch(`${API_URL}/api/decks`, {
    headers: authHeaders(),
  });

  if (!res.ok) {
    throw new Error("Failed to load decks");
  }

  return res.json();
}

// GET SINGLE DECK
export async function getDeck(id: string) {
  const res = await fetch(`${API_URL}/api/decks/${id}`, {
    headers: authHeaders(),
  });

  if (!res.ok) {
    throw new Error("Failed to load deck");
  }

  return res.json();
}
