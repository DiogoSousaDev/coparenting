import { useState, type FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { AuthCard } from '../components/AuthCard';
import { FormField } from '../components/FormField';
import { Button } from '../components/Button';
import { ErrorMessage } from '../components/ErrorMessage';
import { GoogleAuthButton } from '../components/GoogleAuthButton';

export function RegisterPage() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [warning, setWarning] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [searchParams] = useSearchParams();
  const redirect = searchParams.get('redirect');
  const loginHref = redirect ? `/login?redirect=${encodeURIComponent(redirect)}` : '/login';

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const response = await apiFetch<{ warning?: string }>('/api/auth/register', {
        method: 'POST',
        body: { email, password, firstName, lastName },
      });
      setWarning(response.warning ?? null);
      setSubmitted(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao registar.');
    } finally {
      setLoading(false);
    }
  }

  if (submitted) {
    return (
      <AuthCard title="Confirme o seu email">
        {warning ? (
          <p className="mb-4 text-sm text-amber-600">{warning}</p>
        ) : (
          <p className="mb-4 text-sm text-slate-600">
            Enviámos um link de confirmação para <span className="font-medium text-slate-900">{email}</span>.
            Verifique a sua caixa de entrada (ou os logs da API, em desenvolvimento).
          </p>
        )}
        <Link to={loginHref} className="text-sm font-medium text-indigo-600 hover:underline">
          Já confirmou? Iniciar sessão
        </Link>
      </AuthCard>
    );
  }

  return (
    <AuthCard title="Criar conta">
      <form onSubmit={handleSubmit}>
        <FormField
          label="Nome próprio"
          id="firstName"
          required
          value={firstName}
          onChange={(e) => setFirstName(e.target.value)}
        />
        <FormField
          label="Apelido"
          id="lastName"
          required
          value={lastName}
          onChange={(e) => setLastName(e.target.value)}
        />
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
          minLength={8}
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        {error && <ErrorMessage>{error}</ErrorMessage>}
        <Button type="submit" disabled={loading}>
          {loading ? 'A registar…' : 'Registar'}
        </Button>
      </form>

      <div className="my-6 flex items-center gap-3">
        <div className="h-px flex-1 bg-slate-200" />
        <span className="text-xs text-slate-400">ou</span>
        <div className="h-px flex-1 bg-slate-200" />
      </div>

      <GoogleAuthButton onError={setError} />

      <p className="mt-6 text-center text-sm text-slate-600">
        Já tens conta?{' '}
        <Link to={loginHref} className="font-medium text-indigo-600 hover:underline">
          Iniciar sessão
        </Link>
      </p>
    </AuthCard>
  );
}
