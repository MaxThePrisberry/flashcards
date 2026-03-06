import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import Navbar from "@/components/navbar";
import AuthProvider from "@/components/auth-provider";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Flashcards",
  description: "A flashcard study application",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="dark">
      <body className={`${inter.className} min-h-screen antialiased`}>
        <AuthProvider>
          <Navbar />
          <div className="flex flex-col min-h-[calc(100vh-4rem)]">
            {children}
          </div>
        </AuthProvider>
      </body>
    </html>
  );
}
