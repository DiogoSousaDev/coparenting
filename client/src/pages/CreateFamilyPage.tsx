import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';
import { AuthCard } from '../components/AuthCard';
import { FormField } from '../components/FormField';
import { Button } from '../components/Button';
import { ErrorMessage } from '../components/ErrorMessage';

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
    <AuthCard title="Criar unidade familiar">
      <form onSubmit={handleSubmit}>
        <FormField label="Nome da família" id="name" required value={name} onChange={(e) => setName(e.target.value)} />

        <fieldset className="mb-4">
          <legend className="mb-2 block text-sm font-medium text-slate-700">O teu papel</legend>
          <div className="flex gap-4">
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input
                type="radio"
                name="role"
                className="text-indigo-600 focus:ring-indigo-500"
                checked={creatorRole === 0}
                onChange={() => setCreatorRole(0)}
              />
              Pai
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input
                type="radio"
                name="role"
                className="text-indigo-600 focus:ring-indigo-500"
                checked={creatorRole === 1}
                onChange={() => setCreatorRole(1)}
              />
              Mãe
            </label>
          </div>
        </fieldset>

        {error && <ErrorMessage>{error}</ErrorMessage>}
        <Button type="submit" disabled={loading}>
          {loading ? 'A criar…' : 'Criar família'}
        </Button>
      </form>
    </AuthCard>
  );
}
