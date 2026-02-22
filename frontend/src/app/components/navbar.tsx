import Link from "next/link";
import "./navbar.css";

export default function Navbar() {
  return (
    <nav className="navbar">
      <div className="nav-left">
        <Link href="/">Flashcards</Link>
      </div>

      <div className="nav-right">
        <Link href="/login">Login</Link>
        <Link href="/register">Register</Link>
      </div>
    </nav>
  );
}
