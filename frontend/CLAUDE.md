# Frontend Agent Guidelines

## Component & Styling Rules

- Always use **shadcn/ui** components (Button, Input, Card, Label, etc.) — never raw HTML for UI elements
- Always use **Tailwind CSS utility classes** — never inline styles, never raw `.css` files
- Always use **lucide-react** icons — never emoji, SVG files, or other icon libraries
- App logo icon is `Layers` from lucide-react
- **Dark mode only** — `<html>` has `class="dark"`
- Use `cn()` from `@/lib/utils` for conditional class merging

## File Organization

- shadcn components live in `src/components/ui/`
- Custom components live in `src/components/`
- Add new shadcn components via `npx shadcn@latest add <component>`

## Tailwind v4

This project uses **Tailwind CSS v4** (CSS-first configuration via `@theme` directives, no `tailwind.config.ts`).

Do **not** use Tailwind v3 patterns:
- No `@tailwind base/components/utilities` directives
- No `tailwind.config.ts`
- No `tailwindcss-animate` — use `tw-animate-css` instead
- Color format is **OKLCH**, not HSL

## Do Not Modify

- The API layer in `src/app/lib/api/auth.ts`
