import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';

interface InviteDetails {
  familyName: string;
  invitedEmail: string;
  expired: boolean;
  alreadyAccepted: boolean;
}

interface AcceptInviteResponse {
  familyId: string;
}

export function AcceptInvitePage() {
  const { token } = useParams<{ token: string }>();
  const [details, setDetails] = useState<InviteDetails | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [accepting, setAccepting] = useState(false);
  const { token: authToken, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!token) return;
    apiFetch<InviteDetails>(`/api/invites/${token}`)
      .then(setDetails)
      .catch(() => setNotFound(true));
  }, [token]);

  async function handleAccept() {
    if (!token) return;
    setError(null);
    setAccepting(true);
    try {
      await apiFetch<AcceptInviteResponse>(`/api/invites/${token}/accept`, {
        method: 'POST',
        token: authToken,
      });
      navigate('/dashboard');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao aceitar o convite.');
    } finally {
      setAccepting(false);
    }
  }

  if (notFound) {
    return (
      <section>
        <h1>Convite não encontrado</h1>
        <p>Este link de convite é inválido.</p>
      </section>
    );
  }

  if (!details) {
    return <p>A carregar convite…</p>;
  }

  return (
    <section>
      <h1>Convite para {details.familyName}</h1>
      <p>Convite enviado para {details.invitedEmail}.</p>

      {details.alreadyAccepted && <p role="alert">Este convite já foi aceite.</p>}
      {!details.alreadyAccepted && details.expired && <p role="alert">Este convite expirou.</p>}

      {!details.alreadyAccepted && !details.expired && (
        <>
          {isAuthenticated ? (
            <>
              {error && <p role="alert">{error}</p>}
              <button onClick={handleAccept} disabled={accepting}>
                {accepting ? 'A aceitar…' : 'Aceitar convite'}
              </button>
            </>
          ) : (
            <p>
              Precisas de entrar na tua conta primeiro:{' '}
              <Link to={`/login?redirect=${encodeURIComponent(`/accept-invite/${token}`)}`}>Iniciar sessão</Link> ou{' '}
              <Link to={`/register?redirect=${encodeURIComponent(`/accept-invite/${token}`)}`}>criar conta</Link>.
            </p>
          )}
        </>
      )}
    </section>
  );
}
