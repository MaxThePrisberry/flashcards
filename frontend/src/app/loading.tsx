import { Layers } from "lucide-react";

export default function Loading() {
  return (
    <main className="flex items-center justify-center flex-1">
      <Layers className="h-8 w-8 animate-pulse text-muted-foreground" />
    </main>
  );
}
