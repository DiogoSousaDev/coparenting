import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { Calendar, dateFnsLocalizer, Views, type SlotInfo, type View } from 'react-big-calendar';
import { format, parse, startOfWeek, getDay } from 'date-fns';
import { pt } from 'date-fns/locale';
import 'react-big-calendar/lib/css/react-big-calendar.css';
import { apiFetch, ApiError } from '../lib/apiClient';
import { useAuth } from '../auth/AuthContext';
import { useIsMobile } from '../hooks/useIsMobile';
import { EventFormModal, type EventFormValues } from '../components/EventFormModal';
import { ErrorMessage } from '../components/ErrorMessage';
import { MobileCustodyAgenda } from '../components/MobileCustodyAgenda';
import { custodyDayStyle, parseCustodyPeriods, type CustodyPeriod } from '../lib/custody';

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

interface ScheduleSummary {
  paiColor: string;
  maeColor: string;
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

function defaultView(): View {
  return typeof window !== 'undefined' && window.innerWidth < 640 ? Views.AGENDA : Views.MONTH;
}

export function CalendarPage() {
  const { familyId } = useParams<{ familyId: string }>();
  const { token } = useAuth();
  const isMobile = useIsMobile();
  const [familyName, setFamilyName] = useState<string | null>(null);
  const [events, setEvents] = useState<CalendarEventItem[]>([]);
  const [schedule, setSchedule] = useState<ScheduleSummary | null>(null);
  const [custodyPeriods, setCustodyPeriods] = useState<CustodyPeriod[]>([]);
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

  useEffect(() => {
    if (!familyId) return;
    apiFetch<ScheduleSummary | null>(`/api/families/${familyId}/custody-schedule`, { token })
      .then((s) => setSchedule(s ?? null))
      .catch(() => setSchedule(null));
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

  const loadCustodyPeriods = useCallback(() => {
    if (!familyId) return;
    const params = new URLSearchParams({ from: range.start.toISOString(), to: range.end.toISOString() });
    apiFetch<{ start: string; end: string; role: 0 | 1; color: string }[]>(
      `/api/families/${familyId}/custody-periods?${params}`,
      { token },
    )
      .then((data) => setCustodyPeriods(parseCustodyPeriods(data)))
      .catch(() => setCustodyPeriods([]));
  }, [familyId, token, range]);

  useEffect(() => {
    loadEvents();
  }, [loadEvents]);

  useEffect(() => {
    loadCustodyPeriods();
  }, [loadCustodyPeriods]);

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

  function handleSelectDay(date: Date) {
    const start = new Date(date);
    start.setHours(9, 0, 0, 0);
    const end = new Date(date);
    end.setHours(10, 0, 0, 0);
    setModalState({ mode: 'create', start, end });
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

        {schedule ? (
          <div className="mb-3 flex items-center gap-4 text-sm text-slate-600">
            <span className="flex items-center gap-1">
              <span className="inline-block h-3 w-3 rounded-full" style={{ backgroundColor: schedule.paiColor }} />
              Pai
            </span>
            <span className="flex items-center gap-1">
              <span className="inline-block h-3 w-3 rounded-full" style={{ backgroundColor: schedule.maeColor }} />
              Mãe
            </span>
            {familyId && (
              <Link to={`/custody/${familyId}`} className="ml-auto text-indigo-600 hover:underline">
                Configurar guarda
              </Link>
            )}
          </div>
        ) : (
          familyId && (
            <div className="mb-3 flex items-center justify-between rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-700">
              <span>Ainda não configuraste a guarda partilhada.</span>
              <Link to={`/custody/${familyId}`} className="font-medium hover:underline">
                Configurar
              </Link>
            </div>
          )
        )}

        {isMobile ? (
          <MobileCustodyAgenda
            events={events}
            custodyPeriods={custodyPeriods}
            onSelectDay={handleSelectDay}
            onSelectEvent={handleSelectEvent}
          />
        ) : (
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
              dayPropGetter={(date: Date) => ({ style: custodyDayStyle(custodyPeriods, date) })}
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
        )}
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
