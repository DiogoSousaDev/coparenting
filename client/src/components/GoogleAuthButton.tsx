import { GoogleLogin, type CredentialResponse } from '@react-oauth/google';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';

interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  email: string;
}

export function GoogleAuthButton({ onError }: { onError: (message: string) => void }) {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const redirect = searchParams.get('redirect');

  async function handleSuccess(credentialResponse: CredentialResponse) {
    if (!credentialResponse.credential) {
      onError('Não foi possível obter as credenciais da Google.');
      return;
    }

    try {
      const response = await apiFetch<LoginResponse>('/api/auth/google', {
        method: 'POST',
        body: { idToken: credentialResponse.credential },
      });
      login(response.token, response.expiresAtUtc, response.email);
      navigate(redirect ?? '/dashboard');
    } catch (err) {
      onError(err instanceof ApiError ? err.message : 'Erro ao entrar com a Google.');
    }
  }

  return (
    <div className="flex justify-center">
      <GoogleLogin onSuccess={handleSuccess} onError={() => onError('Erro ao entrar com a Google.')} />
    </div>
  );
}
