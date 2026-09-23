import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';
import { formatCycleDuration, type CustodyRole } from '../lib/custody';
import { Button } from '../components/Button';
import { ErrorMessage } from '../components/ErrorMessage';

interface SegmentRow {
  role: CustodyRole;
  days: number;
  hours: number;
}

interface ScheduleResponse {
  anchorStart: string;
  paiColor: string;
  maeColor: string;
  segments: { role: CustodyRole; durationHours: number }[];
}

const WEEKDAY_OPTIONS = [
  { value: 1, label: 'Segunda-feira' },
  { value: 2, label: 'Terça-feira' },
  { value: 3, label: 'Quarta-feira' },
  { value: 4, label: 'Quinta-feira' },
  { value: 5, label: 'Sexta-feira' },
  { value: 6, label: 'Sábado' },
  { value: 0, label: 'Domingo' },
];

const selectClassName =
  'w-full rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500';
const numberInputClassName =
  'w-20 rounded-md border border-slate-300 px-2 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500';

function pad(n: number) {
  return String(n).padStart(2, '0');
}

function toDateInputValue(date: Date) {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

function toTimeInputValue(date: Date) {
  return `${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function nextDateForWeekday(from: Date, weekday: number) {
  const diff = (weekday - from.getDay() + 7) % 7;
  const result = new Date(from);
  result.setDate(from.getDate() + diff);
  return result;
}

function durationHoursToDaysAndHours(totalHours: number) {
  return { days: Math.floor(totalHours / 24), hours: totalHours % 24 };
}

export function CustodySettingsPage() {
  const { familyId } = useParams<{ familyId: string }>();
  const { token } = useAuth();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [anchorDate, setAnchorDate] = useState(() => toDateInputValue(new Date()));
  const [anchorTime, setAnchorTime] = useState('17:00');
  const [segments, setSegments] = useState<SegmentRow[]>([
    { role: 0, days: 7, hours: 0 },
    { role: 1, days: 7, hours: 0 },
  ]);
  const [paiColor, setPaiColor] = useState('#3b82f6');
  const [maeColor, setMaeColor] = useState('#ec4899');

  const [presetWeekday, setPresetWeekday] = useState(5);
  const [presetTime, setPresetTime] = useState('17:00');
  const [presetStartRole, setPresetStartRole] = useState<CustodyRole>(0);

  useEffect(() => {
    if (!familyId) return;
    apiFetch<ScheduleResponse | null>(`/api/families/${familyId}/custody-schedule`, { token })
      .then((schedule) => {
        if (!schedule) return;
        const anchor = new Date(schedule.anchorStart);
        setAnchorDate(toDateInputValue(anchor));
        setAnchorTime(toTimeInputValue(anchor));
        setSegments(
          schedule.segments.map((s) => ({ role: s.role, ...durationHoursToDaysAndHours(s.durationHours) })),
        );
        setPaiColor(schedule.paiColor);
        setMaeColor(schedule.maeColor);
      })
      .catch((err) => setError(err instanceof ApiError ? err.message : 'Erro ao carregar a guarda.'))
      .finally(() => setLoading(false));
  }, [familyId, token]);

  function applyWeekPreset() {
    const anchor = nextDateForWeekday(new Date(), presetWeekday);
    setAnchorDate(toDateInputValue(anchor));
    setAnchorTime(presetTime);
    setSegments([
      { role: presetStartRole, days: 7, hours: 0 },
      { role: presetStartRole === 0 ? 1 : 0, days: 7, hours: 0 },
    ]);
  }

  function updateSegment(index: number, patch: Partial<SegmentRow>) {
    setSegments((prev) => prev.map((s, i) => (i === index ? { ...s, ...patch } : s)));
  }

  function addSegment() {
    const lastRole = segments[segments.length - 1]?.role ?? 0;
    setSegments((prev) => [...prev, { role: lastRole === 0 ? 1 : 0, days: 0, hours: 0 }]);
  }

  function removeSegment(index: number) {
    setSegments((prev) => prev.filter((_, i) => i !== index));
  }

  const totalCycleHours = segments.reduce((sum, s) => sum + s.days * 24 + s.hours, 0);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!familyId) return;
    setError(null);
    setSaving(true);
    try {
      const [year, month, day] = anchorDate.split('-').map(Number);
      const [hour, minute] = anchorTime.split(':').map(Number);
      const anchor = new Date(year, month - 1, day, hour, minute);

      await apiFetch(`/api/families/${familyId}/custody-schedule`, {
        method: 'PUT',
        token,
        body: {
          anchorStart: anchor.toISOString(),
          paiColor,
          maeColor,
          segments: segments.map((s) => ({ role: s.role, durationHours: s.days * 24 + s.hours })),
        },
      });

      navigate(`/calendar/${familyId}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro ao guardar a guarda partilhada.');
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <div className="min-h-screen bg-slate-50 p-4" />;
  }

  return (
    <div className="min-h-screen bg-slate-50 p-4">
      <div className="mx-auto max-w-2xl">
        <Link to={`/calendar/${familyId}`} className="text-sm font-medium text-indigo-600 hover:underline">
          ← Voltar ao calendário
        </Link>
        <h1 className="mb-6 mt-1 text-xl font-semibold text-slate-900">Configurar guarda partilhada</h1>

        <div className="mb-6 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-3 text-sm font-semibold text-slate-700">Início rápido: semana alternada</h2>
          <p className="mb-4 text-xs text-slate-500">
            Troca fixa uma vez por semana, sempre no mesmo dia e hora. Se a vossa guarda tiver outro padrão (ex.
            2-2-3 ou fins de semana alternados), usa os blocos personalizados abaixo.
          </p>
          <div className="flex flex-wrap items-end gap-3">
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700">Dia da troca</label>
              <select
                className={selectClassName}
                value={presetWeekday}
                onChange={(e) => setPresetWeekday(Number(e.target.value))}
              >
                {WEEKDAY_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700">Hora</label>
              <input
                type="time"
                className={selectClassName}
                value={presetTime}
                onChange={(e) => setPresetTime(e.target.value)}
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700">Quem começa</label>
              <select
                className={selectClassName}
                value={presetStartRole}
                onChange={(e) => setPresetStartRole(Number(e.target.value) as CustodyRole)}
              >
                <option value={0}>Pai</option>
                <option value={1}>Mãe</option>
              </select>
            </div>
            <Button type="button" variant="secondary" fullWidth={false} className="px-4" onClick={applyWeekPreset}>
              Usar semana alternada
            </Button>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="mb-3 text-sm font-semibold text-slate-700">Blocos da guarda</h2>

          <div className="mb-4 flex flex-wrap gap-3">
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700">A partir de (data)</label>
              <input
                type="date"
                required
                className={selectClassName}
                value={anchorDate}
                onChange={(e) => setAnchorDate(e.target.value)}
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700">Hora</label>
              <input
                type="time"
                required
                className={selectClassName}
                value={anchorTime}
                onChange={(e) => setAnchorTime(e.target.value)}
              />
            </div>
          </div>

          <div className="space-y-2">
            {segments.map((segment, index) => (
              <div key={index} className="flex items-center gap-2 rounded-md border border-slate-100 p-2">
                <select
                  className={selectClassName}
                  value={segment.role}
                  onChange={(e) => updateSegment(index, { role: Number(e.target.value) as CustodyRole })}
                >
                  <option value={0}>Pai</option>
                  <option value={1}>Mãe</option>
                </select>
                <input
                  type="number"
                  min={0}
                  className={numberInputClassName}
                  value={segment.days}
                  onChange={(e) => updateSegment(index, { days: Number(e.target.value) })}
                />
                <span className="text-xs text-slate-500">dias</span>
                <input
                  type="number"
                  min={0}
                  max={23}
                  className={numberInputClassName}
                  value={segment.hours}
                  onChange={(e) => updateSegment(index, { hours: Number(e.target.value) })}
                />
                <span className="text-xs text-slate-500">horas</span>
                <button
                  type="button"
                  onClick={() => removeSegment(index)}
                  disabled={segments.length <= 1}
                  className="ml-auto text-xs text-red-600 hover:underline disabled:cursor-not-allowed disabled:text-slate-300"
                >
                  Remover
                </button>
              </div>
            ))}
          </div>

          <button type="button" onClick={addSegment} className="mt-3 text-sm text-indigo-600 hover:underline">
            + Adicionar bloco
          </button>

          <p className="mt-3 text-xs text-slate-500">
            Duração total do ciclo: {formatCycleDuration(totalCycleHours)}
            {totalCycleHours === 0 && ' — adiciona pelo menos um bloco com duração maior que zero.'}
          </p>

          <div className="mt-6 flex gap-6 border-t border-slate-100 pt-4">
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700">Cor do Pai</label>
              <input
                type="color"
                value={paiColor}
                onChange={(e) => setPaiColor(e.target.value)}
                className="h-9 w-16 cursor-pointer rounded border border-slate-300"
              />
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-slate-700">Cor da Mãe</label>
              <input
                type="color"
                value={maeColor}
                onChange={(e) => setMaeColor(e.target.value)}
                className="h-9 w-16 cursor-pointer rounded border border-slate-300"
              />
            </div>
          </div>

          {error && (
            <div className="mt-4">
              <ErrorMessage>{error}</ErrorMessage>
            </div>
          )}

          <div className="mt-6">
            <Button type="submit" disabled={saving || totalCycleHours === 0}>
              {saving ? 'A guardar…' : 'Guardar guarda partilhada'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
