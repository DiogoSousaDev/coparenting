import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { AuthCard } from '../components/AuthCard';
import { ErrorMessage } from '../components/ErrorMessage';

type Status = 'loading' | 'success' | 'error';

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const [status, setStatus] = useState<Status>('loading');
  const [message, setMessage] = useState('');

  useEffect(() => {
    const userId = searchParams.get('userId');
    const token = searchParams.get('token');

    if (!userId || !token) {
      setStatus('error');
      setMessage('Link de confirmação inválido.');
      return;
    }

    const query = new URLSearchParams({ userId, token }).toString();
    apiFetch(`/api/auth/confirm-email?${query}`)
      .then(() => setStatus('success'))
      .catch((err) => {
        setStatus('error');
        setMessage(err instanceof ApiError ? err.message : 'Erro ao confirmar o email.');
      });
  }, [searchParams]);

  return (
    <AuthCard title="Confirmação de email">
      {status === 'loading' && <p className="text-sm text-slate-600">A confirmar…</p>}
      {status === 'success' && (
        <>
          <p className="mb-4 text-sm text-slate-600">Email confirmado com sucesso.</p>
          <Link to="/login" className="text-sm font-medium text-indigo-600 hover:underline">
            Iniciar sessão
          </Link>
        </>
      )}
      {status === 'error' && <ErrorMessage>{message}</ErrorMessage>}
    </AuthCard>
  );
}
