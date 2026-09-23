import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { Calendar, dateFnsLocalizer, Views, type SlotInfo, type View } from 'react-big-calendar';
import { format, parse, startOfWeek, getDay } from 'date-fns';
import { pt } from 'date-fns/locale';
import 'react-big-calendar/lib/css/react-big-calendar.css';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';
import { EventFormModal, type EventFormValues } from '../components/EventFormModal';
import { ErrorMessage } from '../components/ErrorMessage';

const localizer = dateFnsLocalizer({
  format,
  parse,
  startOfWeek: (date: Date) => startOfWeek(date, { locale: pt }),
  getDay,
  locales: { pt },
});

interface EventOccurrenceDto {
  eventId: string;
  title: string;
  start: string;
  end: string;
  isRecurring: boolean;
  recurrenceRule: string | null;
}

interface CalendarEventItem {
  eventId: string;
  title: string;
  start: Date;
  end: Date;
  isRecurring: boolean;
  recurrenceRule: string | null;
}

interface FamilySummary {
  familyId: string;
  name: string;
}

type ModalState =
  | { mode: 'create'; start: Date; end: Date }
  | { mode: 'edit'; event: CalendarEventItem }
  | null;

function defaultRange() {
  const now = new Date();
  return {
    start: new Date(now.getFullYear(), now.getMonth() - 1, 1),
    end: new Date(now.getFullYear(), now.getMonth() + 2, 0),
  };
}

const MOBILE_BREAKPOINT_PX = 640;

function defaultView(): View {
  return typeof window !== 'undefined' && window.innerWidth < MOBILE_BREAKPOINT_PX ? Views.AGENDA : Views.MONTH;
}

export function CalendarPage() {
  const { familyId } = useParams<{ familyId: string }>();
  const { token } = useAuth();
  const [familyName, setFamilyName] = useState<string | null>(null);
  const [events, setEvents] = useState<CalendarEventItem[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [range, setRange] = useState(defaultRange);
  const [currentDate, setCurrentDate] = useState(new Date());
  const [view, setView] = useState<View>(defaultView);
  const [modalState, setModalState] = useState<ModalState>(null);

  useEffect(() => {
    apiFetch<FamilySummary[]>('/api/families', { token })
      .then((families) => setFamilyName(families.find((f) => f.familyId === familyId)?.name ?? null))
      .catch(() => setFamilyName(null));
  }, [familyId, token]);

  const loadEvents = useCallback(() => {
    if (!familyId) return;
    const params = new URLSearchParams({ from: range.start.toISOString(), to: range.end.toISOString() });
    apiFetch<EventOccurrenceDto[]>(`/api/families/${familyId}/calendar-events?${params}`, { token })
      .then((data) =>
        setEvents(
          data.map((e) => ({
            ...e,
            start: new Date(e.start),
            end: new Date(e.end),
          })),
        ),
      )
      .catch((err) => setError(err instanceof ApiError ? err.message : 'Erro ao carregar eventos.'));
  }, [familyId, token, range]);

  useEffect(() => {
    loadEvents();
  }, [loadEvents]);

  function handleRangeChange(newRange: Date[] | { start: Date; end: Date }) {
    if (Array.isArray(newRange)) {
      if (newRange.length === 0) return;
      setRange({ start: newRange[0], end: newRange[newRange.length - 1] });
    } else {
      setRange(newRange);
    }
  }

  function handleSelectSlot(slotInfo: SlotInfo) {
    setModalState({ mode: 'create', start: slotInfo.start, end: slotInfo.end });
  }

  function handleSelectEvent(event: CalendarEventItem) {
    setModalState({ mode: 'edit', event });
  }

  async function handleSave(values: EventFormValues) {
    if (!familyId || !modalState) return;

    if (modalState.mode === 'edit') {
      await apiFetch(`/api/calendar-events/${modalState.event.eventId}`, {
        method: 'PUT',
        token,
        body: values,
      });
    } else {
      await apiFetch(`/api/families/${familyId}/calendar-events`, {
        method: 'POST',
        token,
        body: values,
      });
    }

    setModalState(null);
    loadEvents();
  }

  async function handleDelete() {
    if (modalState?.mode !== 'edit') return;
    await apiFetch(`/api/calendar-events/${modalState.event.eventId}`, { method: 'DELETE', token });
    setModalState(null);
    loadEvents();
  }

  return (
    <div className="min-h-screen bg-slate-50 p-4">
      <div className="mx-auto max-w-5xl">
        <div className="mb-4 flex items-center justify-between">
          <div>
            <Link to="/dashboard" className="text-sm font-medium text-indigo-600 hover:underline">
              ← Voltar ao dashboard
            </Link>
            <h1 className="text-xl font-semibold text-slate-900">{familyName ?? 'Calendário'}</h1>
          </div>
        </div>

        {error && <ErrorMessage>{error}</ErrorMessage>}

        <div className="h-[75vh] min-h-[420px] rounded-xl border border-slate-200 bg-white p-2 shadow-sm sm:h-[700px] sm:p-4">
          <Calendar
            localizer={localizer}
            events={events}
            startAccessor="start"
            endAccessor="end"
            titleAccessor="title"
            selectable
            date={currentDate}
            onNavigate={setCurrentDate}
            view={view}
            onView={setView}
            views={[Views.MONTH, Views.WEEK, Views.DAY, Views.AGENDA]}
            onSelectSlot={handleSelectSlot}
            onSelectEvent={handleSelectEvent}
            onRangeChange={handleRangeChange}
            culture="pt"
            messages={{
              next: 'Seguinte',
              previous: 'Anterior',
              today: 'Hoje',
              month: 'Mês',
              week: 'Semana',
              day: 'Dia',
              agenda: 'Agenda',
              date: 'Data',
              time: 'Hora',
              event: 'Evento',
              noEventsInRange: 'Sem eventos neste período',
              showMore: (total) => `+${total} mais`,
            }}
          />
        </div>
      </div>

      {modalState && (
        <EventFormModal
          initial={
            modalState.mode === 'edit'
              ? {
                  title: modalState.event.title,
                  start: modalState.event.start,
                  end: modalState.event.end,
                  recurrenceRule: modalState.event.recurrenceRule,
                }
              : { title: '', start: modalState.start, end: modalState.end, recurrenceRule: null }
          }
          isEditing={modalState.mode === 'edit'}
          onSave={handleSave}
          onDelete={modalState.mode === 'edit' ? handleDelete : undefined}
          onClose={() => setModalState(null)}
        />
      )}
    </div>
  );
}
