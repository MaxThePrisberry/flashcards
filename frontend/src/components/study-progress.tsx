import { cn } from "@/lib/utils";
import { Card, CardContent } from "@/components/ui/card";

interface StudyProgressProps {
  title: string;
  description?: string;
  currentIndex: number;
  currentCardNumber: number;
  totalCards: number;
  completed: boolean;
}

export default function StudyProgress({
  title,
  description,
  currentIndex,
  currentCardNumber,
  totalCards,
  completed,
}: StudyProgressProps) {
  return (
    <Card className="border-border/70 bg-card/80">
      <CardContent className="space-y-5 p-6">
        <div className="space-y-1">
          <p className="text-sm text-muted-foreground">Study mode</p>
          <h1 className="text-3xl font-semibold">{title}</h1>
          {description ? (
            <p className="max-w-3xl text-sm text-muted-foreground">
              {description}
            </p>
          ) : null}
        </div>

        <div className="flex items-center justify-between gap-4">
          <div>
            <p className="text-sm text-muted-foreground">Progress</p>
            <p className="text-lg font-medium">
              {completed
                ? `${totalCards} / ${totalCards}`
                : `${currentCardNumber} / ${totalCards}`}
            </p>
          </div>

          <p className="text-sm text-muted-foreground">
            {completed ? "Complete" : `${currentIndex} reviewed`}
          </p>
        </div>

        <div className="grid grid-cols-12 gap-2 sm:grid-cols-[repeat(auto-fit,minmax(0,1fr))]">
          {Array.from({ length: totalCards }).map((_, index) => {
            const isDone = index < currentIndex;
            const isCurrent = !completed && index === currentIndex;

            return (
              <div
                key={index}
                className={cn(
                  "h-2 rounded-full transition-colors",
                  isDone && "bg-primary",
                  isCurrent && "bg-primary/50",
                  !isDone && !isCurrent && "bg-muted",
                )}
              />
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
