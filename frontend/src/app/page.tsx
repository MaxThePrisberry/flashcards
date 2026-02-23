import Link from "next/link";
import { Layers } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function Home() {
  return (
    <main className="flex flex-col items-center justify-center min-h-[calc(100vh-4rem)] gap-6 text-center px-4">
      <Layers className="h-16 w-16 text-primary" />
      <h1 className="text-4xl font-bold tracking-tight">Flashcards</h1>
      <p className="text-lg text-muted-foreground">
        Create decks. Study smarter. Track progress.
      </p>

      <div className="flex gap-4">
        <Button asChild size="lg">
          <Link href="/login">Login</Link>
        </Button>
        <Button asChild variant="outline" size="lg">
          <Link href="/register">Register</Link>
        </Button>
      </div>
    </main>
  );
}
