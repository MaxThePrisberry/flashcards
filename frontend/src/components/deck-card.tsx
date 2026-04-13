"use client";

import Link from "next/link";
import { Heart } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface Props {
  id: string;
  title: string;
  description: string;
  cardCount: number;
  likeCount: number;
  showLikeButton?: boolean;
  isLiked?: boolean;
  liking?: boolean;
  onToggleLike?: (id: string) => void;
}

export default function DeckCard({
  id,
  title,
  description,
  cardCount,
  likeCount,
  showLikeButton = false,
  isLiked = false,
  liking = false,
  onToggleLike,
}: Props) {
  return (
    <Card className="transition hover:bg-accent/40">
      <CardHeader className="flex flex-row items-start justify-between gap-4">
        <Link href={`/decks/${id}`} className="min-w-0 flex-1 space-y-1">
          <CardTitle className="truncate">{title}</CardTitle>
          <CardDescription className="line-clamp-2">
            {description || "No description"} • {cardCount} cards • {likeCount}{" "}
            like{likeCount === 1 ? "" : "s"}
          </CardDescription>
        </Link>

        {showLikeButton ? (
          <Button
            type="button"
            variant={isLiked ? "secondary" : "outline"}
            size="sm"
            className="shrink-0 gap-2"
            disabled={liking}
            onClick={(event) => {
              event.preventDefault();
              event.stopPropagation();
              onToggleLike?.(id);
            }}
          >
            <Heart className={cn("h-4 w-4", isLiked && "fill-current")} />
            {isLiked ? "Liked" : "Like"}
          </Button>
        ) : null}
      </CardHeader>
    </Card>
  );
}
