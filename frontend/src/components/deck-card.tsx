import Link from "next/link";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";

interface Props {
  id: string;
  title: string;
  description: string;
  cardCount: number;
}

export default function DeckCard({ id, title, description, cardCount }: Props) {
  return (
    <Link href={`/decks/${id}`}>
      <Card className="hover:bg-accent transition cursor-pointer">
        <CardHeader>
          <CardTitle>{title}</CardTitle>
          <CardDescription>
            {description || "No description"} • {cardCount} cards
          </CardDescription>
        </CardHeader>
      </Card>
    </Link>
  );
}
