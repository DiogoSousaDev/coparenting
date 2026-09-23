import { useState, type FormEvent } from 'react';
import { Button } from './Button';
import { FormField } from './FormField';
import { ErrorMessage } from './ErrorMessage';

export interface EventFormValues {
  title: string;
  start: string;
  end: string;
  recurrenceRule: string | null;
}

interface EventFormModalProps {
  initial: { title: string; start: Date; end: Date; recurrenceRule: string | null };
  isEditing: boolean;
  onSave: (values: EventFormValues) => Promise<void>;
  onDelete?: () => Promise<void>;
  onClose: () => void;
}

function toLocalInputValue(date: Date) {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

const RECURRENCE_OPTIONS = [
  { value: '', label: 'Não se repete' },
  { value: 'FREQ=WEEKLY;INTERVAL=1', label: 'Repete toda a semana' },
  { value: 'FREQ=WEEKLY;INTERVAL=2', label: 'Repete a cada 2 semanas' },
];

export function EventFormModal({ initial, isEditing, onSave, onDelete, onClose }: EventFormModalProps) {
  const [title, setTitle] = useState(initial.title);
  const [start, setStart] = useState(toLocalInputValue(initial.start));
  const [end, setEnd] = useState(toLocalInputValue(initial.end));
  const [recurrenceRule, setRecurrenceRule] = useState(initial.recurrenceRule ?? '');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [deleting, setDeleting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await onSave({
        title,
        start: new Date(start).toISOString(),
        end: new Date(end).toISOString(),
        recurrenceRule: recurrenceRule || null,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao guardar o evento.');
    } finally {
      setLoading(false);
    }
  }

  async function handleDelete() {
    if (!onDelete) return;
    setError(null);
    setDeleting(true);
    try {
      await onDelete();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao apagar o evento.');
      setDeleting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30 px-4" onClick={onClose}>
      <div className="w-full max-w-sm rounded-xl bg-white p-6 shadow-lg" onClick={(e) => e.stopPropagation()}>
        <h2 className="mb-4 text-lg font-semibold text-slate-900">{isEditing ? 'Editar evento' : 'Novo evento'}</h2>
        <form onSubmit={handleSubmit}>
          <FormField label="Título" id="event-title" required value={title} onChange={(e) => setTitle(e.target.value)} />
          <FormField
            label="Início"
            id="event-start"
            type="datetime-local"
            required
            value={start}
            onChange={(e) => setStart(e.target.value)}
          />
          <FormField
            label="Fim"
            id="event-end"
            type="datetime-local"
            required
            value={end}
            onChange={(e) => setEnd(e.target.value)}
          />

          <div className="mb-4">
            <label htmlFor="event-recurrence" className="mb-1 block text-sm font-medium text-slate-700">
              Repetição
            </label>
            <select
              id="event-recurrence"
              value={recurrenceRule}
              onChange={(e) => setRecurrenceRule(e.target.value)}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-900 focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            >
              {RECURRENCE_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>

          {isEditing && recurrenceRule && (
            <p className="mb-4 text-xs text-amber-600">Editar aqui altera toda a série de eventos recorrentes.</p>
          )}

          {error && <ErrorMessage>{error}</ErrorMessage>}

          <div className="flex gap-2">
            <Button type="submit" disabled={loading}>
              {loading ? 'A guardar…' : 'Guardar'}
            </Button>
            {isEditing && onDelete && (
              <Button
                type="button"
                variant="secondary"
                fullWidth={false}
                className="px-4"
                onClick={handleDelete}
                disabled={deleting}
              >
                {deleting ? 'A apagar…' : 'Apagar'}
              </Button>
            )}
          </div>
        </form>
        <button type="button" onClick={onClose} className="mt-4 text-sm text-slate-500 hover:underline">
          Cancelar
        </button>
      </div>
    </div>
  );
}
