import Link from "next/link";
import { Layers } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function Navbar() {
  return (
    <nav className="h-16 border-b border-border bg-background flex items-center justify-between px-6">
      <Link href="/" className="flex items-center gap-2 text-lg font-semibold">
        <Layers className="h-5 w-5" />
        Flashcards
      </Link>

      <div className="flex items-center gap-2">
        <Button variant="ghost" asChild>
          <Link href="/login">Login</Link>
        </Button>
        <Button variant="ghost" asChild>
          <Link href="/register">Register</Link>
        </Button>
      </div>
    </nav>
  );
}
