import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';

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
    <section>
      <h1>Confirmação de email</h1>
      {status === 'loading' && <p>A confirmar…</p>}
      {status === 'success' && (
        <>
          <p>Email confirmado com sucesso.</p>
          <Link to="/login">Iniciar sessão</Link>
        </>
      )}
      {status === 'error' && <p role="alert">{message}</p>}
    </section>
  );
}
