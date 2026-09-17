import { useEffect, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';
import { FormField } from '../components/FormField';
import { Button } from '../components/Button';
import { ErrorMessage } from '../components/ErrorMessage';

interface FamilySummary {
  familyId: string;
  name: string;
  role: 0 | 1;
}

const roleLabel = (role: 0 | 1) => (role === 0 ? 'Pai' : 'Mãe');

function InviteForm({
  familyId,
  token,
  currentUserRole,
}: {
  familyId: string;
  token: string | null;
  currentUserRole: 0 | 1;
}) {
  const [email, setEmail] = useState('');
  const [role, setRole] = useState<0 | 1>(currentUserRole === 0 ? 1 : 0);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setMessage(null);
    setLoading(true);
    try {
      await apiFetch(`/api/families/${familyId}/invite`, {
        method: 'POST',
        token,
        body: { email, role },
      });
      setMessage(`Convite enviado para ${email}.`);
      setEmail('');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao enviar convite.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="mt-4 border-t border-slate-100 pt-4">
      <p className="mb-3 text-sm font-medium text-slate-700">Convidar o outro progenitor</p>
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <div className="flex-1">
          <FormField
            label="Email"
            id={`invite-email-${familyId}`}
            type="email"
            placeholder="email@exemplo.com"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>
        <div className="mb-4">
          <label htmlFor={`invite-role-${familyId}`} className="mb-1 block text-sm font-medium text-slate-700">
            Papel
          </label>
          <select
            id={`invite-role-${familyId}`}
            value={role}
            onChange={(e) => setRole(Number(e.target.value) as 0 | 1)}
            className="rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
          >
            <option value={0}>Pai</option>
            <option value={1}>Mãe</option>
          </select>
        </div>
        <div className="mb-4">
          <Button type="submit" disabled={loading} fullWidth={false} className="px-6">
            {loading ? 'A enviar…' : 'Convidar'}
          </Button>
        </div>
      </div>
      {message && <p className="text-sm text-emerald-600">{message}</p>}
      {error && <ErrorMessage>{error}</ErrorMessage>}
    </form>
  );
}

export function DashboardPage() {
  const [families, setFamilies] = useState<FamilySummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const { token, email, logout } = useAuth();

  useEffect(() => {
    apiFetch<FamilySummary[]>('/api/families', { token })
      .then(setFamilies)
      .catch((err) => setError(err instanceof ApiError ? err.message : 'Erro ao carregar famílias.'));
  }, [token]);

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-3xl items-center justify-between px-4 py-4">
          <div>
            <p className="text-sm text-slate-500">Olá,</p>
            <p className="font-medium text-slate-900">{email}</p>
          </div>
          <Button variant="secondary" fullWidth={false} className="px-4" onClick={logout}>
            Terminar sessão
          </Button>
        </div>
      </header>

      <main className="mx-auto max-w-3xl px-4 py-8">
        {error && <ErrorMessage>{error}</ErrorMessage>}

        {families && families.length === 0 && (
          <div className="rounded-xl border border-dashed border-slate-300 bg-white p-8 text-center">
            <p className="mb-4 text-sm text-slate-600">Ainda não pertences a nenhuma família.</p>
            <Link
              to="/create-family"
              className="inline-block rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
            >
              Criar uma unidade familiar
            </Link>
          </div>
        )}

        <div className="space-y-4">
          {families?.map((family) => (
            <article key={family.familyId} className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
              <h2 className="text-lg font-semibold text-slate-900">{family.name}</h2>
              <p className="text-sm text-slate-500">O teu papel: {roleLabel(family.role)}</p>
              <InviteForm familyId={family.familyId} token={token} currentUserRole={family.role} />
            </article>
          ))}
        </div>

        {families && families.length > 0 && (
          <Link to="/create-family" className="mt-6 inline-block text-sm font-medium text-indigo-600 hover:underline">
            Criar outra unidade familiar
          </Link>
        )}
      </main>
    </div>
  );
}
