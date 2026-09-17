import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';

interface StoredAuth {
  token: string;
  expiresAtUtc: string;
  email: string;
}

interface AuthContextValue {
  token: string | null;
  email: string | null;
  isAuthenticated: boolean;
  login: (token: string, expiresAtUtc: string, email: string) => void;
  logout: () => void;
}

const STORAGE_KEY = 'coparenting.auth';

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function readStoredAuth(): StoredAuth | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;

  try {
    const parsed = JSON.parse(raw) as StoredAuth;
    if (new Date(parsed.expiresAtUtc) <= new Date()) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
    return parsed;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<StoredAuth | null>(() => readStoredAuth());

  const value = useMemo<AuthContextValue>(
    () => ({
      token: auth?.token ?? null,
      email: auth?.email ?? null,
      isAuthenticated: auth !== null,
      login: (token, expiresAtUtc, email) => {
        const next = { token, expiresAtUtc, email };
        localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
        setAuth(next);
      },
      logout: () => {
        localStorage.removeItem(STORAGE_KEY);
        setAuth(null);
      },
    }),
    [auth],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider');
  }
  return context;
}
