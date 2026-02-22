import { clear } from "console";
import Link from "next/link";

export default function Home() {
  return (
    <main
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100vh",
        gap: "1.5rem",
        textAlign: "center",
      }}
    >
      <h1>Flashcards</h1>
      <p>Create decks. Study smarter. Track progress.</p>

      <div style={{ display: "flex", gap: "1rem" }}>
        <Link href="/login">
          <button style={{ padding: "0.6rem 1.2rem" }}>Login</button>
        </Link>

        <Link href="/register">
          <button style={{ padding: "0.6rem 1.2rem" }}>Register</button>
        </Link>
      </div>
    </main>
  );
}
