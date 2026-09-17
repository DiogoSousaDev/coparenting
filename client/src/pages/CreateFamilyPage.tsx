import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';

interface CreateFamilyResponse {
  familyId: string;
}

export function CreateFamilyPage() {
  const [name, setName] = useState('');
  const [creatorRole, setCreatorRole] = useState<0 | 1>(0);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { token } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await apiFetch<CreateFamilyResponse>('/api/families', {
        method: 'POST',
        token,
        body: { name, creatorRole },
      });
      navigate('/dashboard');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao criar família.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <section>
      <h1>Criar unidade familiar</h1>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="name">Nome da família</label>
          <input id="name" required value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <fieldset>
          <legend>O teu papel</legend>
          <label>
            <input type="radio" name="role" checked={creatorRole === 0} onChange={() => setCreatorRole(0)} />
            Pai
          </label>
          <label>
            <input type="radio" name="role" checked={creatorRole === 1} onChange={() => setCreatorRole(1)} />
            Mãe
          </label>
        </fieldset>
        {error && <p role="alert">{error}</p>}
        <button type="submit" disabled={loading}>
          {loading ? 'A criar…' : 'Criar família'}
        </button>
      </form>
    </section>
  );
}
