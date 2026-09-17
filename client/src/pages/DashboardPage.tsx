import { useEffect, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';

interface FamilySummary {
  familyId: string;
  name: string;
  role: 0 | 1;
}

const roleLabel = (role: 0 | 1) => (role === 0 ? 'Pai' : 'Mãe');

function InviteForm({ familyId, token }: { familyId: string; token: string | null }) {
  const [email, setEmail] = useState('');
  const [role, setRole] = useState<0 | 1>(1);
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
    <form onSubmit={handleSubmit}>
      <input
        type="email"
        placeholder="Email do outro progenitor"
        required
        value={email}
        onChange={(e) => setEmail(e.target.value)}
      />
      <select value={role} onChange={(e) => setRole(Number(e.target.value) as 0 | 1)}>
        <option value={0}>Pai</option>
        <option value={1}>Mãe</option>
      </select>
      <button type="submit" disabled={loading}>
        {loading ? 'A enviar…' : 'Convidar'}
      </button>
      {message && <p>{message}</p>}
      {error && <p role="alert">{error}</p>}
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
    <section>
      <header>
        <h1>Olá, {email}</h1>
        <button onClick={logout}>Terminar sessão</button>
      </header>

      {error && <p role="alert">{error}</p>}

      {families && families.length === 0 && (
        <p>
          Ainda não pertences a nenhuma família. <Link to="/create-family">Criar uma unidade familiar</Link>.
        </p>
      )}

      {families?.map((family) => (
        <article key={family.familyId}>
          <h2>{family.name}</h2>
          <p>O teu papel: {roleLabel(family.role)}</p>
          <InviteForm familyId={family.familyId} token={token} />
        </article>
      ))}

      {families && families.length > 0 && <Link to="/create-family">Criar outra unidade familiar</Link>}
    </section>
  );
}
