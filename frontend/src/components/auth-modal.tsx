"use client";

import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import LoginForm from "@/components/login-form";
import RegisterForm from "@/components/register-form";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";

type AuthMode = "login" | "register";

export default function AuthModal({
  isOpen,
  onClose,
  initialMode = "login",
}: {
  isOpen: boolean;
  onClose: () => void;
  initialMode?: AuthMode;
}) {
  const [mode, setMode] = useState<AuthMode>(initialMode);

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm px-4"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
        >
          <motion.div
            initial={{ opacity: 0, y: 24, scale: 0.96 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 24, scale: 0.96 }}
            transition={{ duration: 0.2 }}
          >
            <Card className="w-full max-w-sm">
              <CardHeader>
                <CardTitle className="text-2xl">
                  {mode === "login" ? "Login" : "Register"}
                </CardTitle>
                <CardDescription>
                  {mode === "login"
                    ? "Enter your credentials to access your account"
                    : "Create an account to start studying"}
                </CardDescription>
              </CardHeader>

              <CardContent>
                {mode === "login" ? (
                  <LoginForm onSuccess={onClose} />
                ) : (
                  <RegisterForm onSuccess={onClose} />
                )}
              </CardContent>

              <CardFooter className="justify-center">
                {mode === "login" ? (
                  <p className="text-sm text-muted-foreground">
                    Don&apos;t have an account?{" "}
                    <Button
                      variant="link"
                      className="p-0 h-auto"
                      onClick={() => setMode("register")}
                    >
                      Register
                    </Button>
                  </p>
                ) : (
                  <p className="text-sm text-muted-foreground">
                    Already have an account?{" "}
                    <Button
                      variant="link"
                      className="p-0 h-auto"
                      onClick={() => setMode("login")}
                    >
                      Login
                    </Button>
                  </p>
                )}
              </CardFooter>
            </Card>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
