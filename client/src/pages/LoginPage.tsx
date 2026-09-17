import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';

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
    <section>
      <h1>Iniciar sessão</h1>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="email">Email</label>
          <input id="email" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div>
          <label htmlFor="password">Password</label>
          <input id="password" type="password" required value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        {error && <p role="alert">{error}</p>}
        <button type="submit" disabled={loading}>
          {loading ? 'A entrar…' : 'Entrar'}
        </button>
      </form>
      <p>
        Ainda não tens conta?{' '}
        <Link to={redirect ? `/register?redirect=${encodeURIComponent(redirect)}` : '/register'}>Registar</Link>
      </p>
    </section>
  );
}
