import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';
import { AuthCard } from '../components/AuthCard';
import { Button } from '../components/Button';
import { ErrorMessage } from '../components/ErrorMessage';

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
      <AuthCard title="Convite não encontrado">
        <p className="text-sm text-slate-600">Este link de convite é inválido.</p>
      </AuthCard>
    );
  }

  if (!details) {
    return (
      <AuthCard title="Convite">
        <p className="text-sm text-slate-600">A carregar convite…</p>
      </AuthCard>
    );
  }

  return (
    <AuthCard title={`Convite para ${details.familyName}`}>
      <p className="mb-4 text-sm text-slate-600">
        Convite enviado para <span className="font-medium text-slate-900">{details.invitedEmail}</span>.
      </p>

      {details.alreadyAccepted && <ErrorMessage>Este convite já foi aceite.</ErrorMessage>}
      {!details.alreadyAccepted && details.expired && <ErrorMessage>Este convite expirou.</ErrorMessage>}

      {!details.alreadyAccepted && !details.expired && (
        <>
          {isAuthenticated ? (
            <>
              {error && <ErrorMessage>{error}</ErrorMessage>}
              <Button onClick={handleAccept} disabled={accepting}>
                {accepting ? 'A aceitar…' : 'Aceitar convite'}
              </Button>
            </>
          ) : (
            <p className="text-sm text-slate-600">
              Precisas de entrar na tua conta primeiro:{' '}
              <Link
                to={`/login?redirect=${encodeURIComponent(`/accept-invite/${token}`)}`}
                className="font-medium text-indigo-600 hover:underline"
              >
                Iniciar sessão
              </Link>{' '}
              ou{' '}
              <Link
                to={`/register?redirect=${encodeURIComponent(`/accept-invite/${token}`)}`}
                className="font-medium text-indigo-600 hover:underline"
              >
                criar conta
              </Link>
              .
            </p>
          )}
        </>
      )}
    </AuthCard>
  );
}
