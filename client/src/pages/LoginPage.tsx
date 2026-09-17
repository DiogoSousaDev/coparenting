import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';
import { AuthCard } from '../components/AuthCard';
import { FormField } from '../components/FormField';
import { Button } from '../components/Button';
import { ErrorMessage } from '../components/ErrorMessage';

interface LoginResponse {
  token: string;
  expiresAtUtc: string;
}

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const redirect = searchParams.get('redirect');

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const response = await apiFetch<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: { email, password },
      });
      login(response.token, response.expiresAtUtc, email);
      navigate(redirect ?? '/dashboard');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao iniciar sessão.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthCard title="Iniciar sessão">
      <form onSubmit={handleSubmit}>
        <FormField
          label="Email"
          id="email"
          type="email"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <FormField
          label="Password"
          id="password"
          type="password"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        {error && <ErrorMessage>{error}</ErrorMessage>}
        <Button type="submit" disabled={loading}>
          {loading ? 'A entrar…' : 'Entrar'}
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-slate-600">
        Ainda não tens conta?{' '}
        <Link
          to={redirect ? `/register?redirect=${encodeURIComponent(redirect)}` : '/register'}
          className="font-medium text-indigo-600 hover:underline"
        >
          Registar
        </Link>
      </p>
    </AuthCard>
  );
}
