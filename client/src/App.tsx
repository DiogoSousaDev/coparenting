import { GoogleOAuthProvider } from '@react-oauth/google';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './auth/AuthContext';
import { RequireAuth } from './auth/RequireAuth';
import { RegisterPage } from './pages/RegisterPage';
import { LoginPage } from './pages/LoginPage';
import { ConfirmEmailPage } from './pages/ConfirmEmailPage';
import { CreateFamilyPage } from './pages/CreateFamilyPage';
import { AcceptInvitePage } from './pages/AcceptInvitePage';
import { DashboardPage } from './pages/DashboardPage';
import { CalendarPage } from './pages/CalendarPage';
import { CustodySettingsPage } from './pages/CustodySettingsPage';

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID as string;

function App() {
  return (
    <GoogleOAuthProvider clientId={googleClientId}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/confirm-email" element={<ConfirmEmailPage />} />
            <Route path="/accept-invite/:token" element={<AcceptInvitePage />} />
            <Route
              path="/dashboard"
              element={
                <RequireAuth>
                  <DashboardPage />
                </RequireAuth>
              }
            />
            <Route
              path="/create-family"
              element={
                <RequireAuth>
                  <CreateFamilyPage />
                </RequireAuth>
              }
            />
            <Route
              path="/calendar/:familyId"
              element={
                <RequireAuth>
                  <CalendarPage />
                </RequireAuth>
              }
            />
            <Route
              path="/custody/:familyId"
              element={
                <RequireAuth>
                  <CustodySettingsPage />
                </RequireAuth>
              }
            />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </GoogleOAuthProvider>
  );
}

export default App;
