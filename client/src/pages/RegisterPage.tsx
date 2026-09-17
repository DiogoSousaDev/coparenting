import { useState, type FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';

export function RegisterPage() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [loading, setLoading] = useState(false);
  const [searchParams] = useSearchParams();
  const redirect = searchParams.get('redirect');
  const loginHref = redirect ? `/login?redirect=${encodeURIComponent(redirect)}` : '/login';

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await apiFetch('/api/auth/register', {
        method: 'POST',
        body: { email, password, firstName, lastName },
      });
      setSubmitted(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao registar.');
    } finally {
      setLoading(false);
    }
  }

  if (submitted) {
    return (
      <section>
        <h1>Confirme o seu email</h1>
        <p>Enviámos um link de confirmação para {email}. Verifique a sua caixa de entrada (ou os logs da API, em desenvolvimento).</p>
        <Link to={loginHref}>Já confirmou? Iniciar sessão</Link>
      </section>
    );
  }

  return (
    <section>
      <h1>Criar conta</h1>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="firstName">Nome próprio</label>
          <input id="firstName" required value={firstName} onChange={(e) => setFirstName(e.target.value)} />
        </div>
        <div>
          <label htmlFor="lastName">Apelido</label>
          <input id="lastName" required value={lastName} onChange={(e) => setLastName(e.target.value)} />
        </div>
        <div>
          <label htmlFor="email">Email</label>
          <input id="email" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </div>
        <div>
          <label htmlFor="password">Password</label>
          <input id="password" type="password" required minLength={8} value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        {error && <p role="alert">{error}</p>}
        <button type="submit" disabled={loading}>
          {loading ? 'A registar…' : 'Registar'}
        </button>
      </form>
      <p>
        Já tens conta? <Link to={loginHref}>Iniciar sessão</Link>
      </p>
    </section>
  );
}
