import { useState } from 'react';
import { addWeeks, format, isSameDay, startOfWeek } from 'date-fns';
import { pt } from 'date-fns/locale';
import { custodyDayStyle, type CustodyPeriod } from '../lib/custody';

interface AgendaEvent {
  eventId: string;
  title: string;
  start: Date;
  end: Date;
}

interface MobileCustodyAgendaProps<TEvent extends AgendaEvent> {
  events: TEvent[];
  custodyPeriods: CustodyPeriod[];
  onSelectDay: (date: Date) => void;
  onSelectEvent: (event: TEvent) => void;
}

export function MobileCustodyAgenda<TEvent extends AgendaEvent>({
  events,
  custodyPeriods,
  onSelectDay,
  onSelectEvent,
}: MobileCustodyAgendaProps<TEvent>) {
  const [weekStart, setWeekStart] = useState(() => startOfWeek(new Date(), { locale: pt }));
  const [selectedDay, setSelectedDay] = useState(() => new Date());

  const days = Array.from({ length: 7 }, (_, i) => {
    const date = new Date(weekStart);
    date.setDate(weekStart.getDate() + i);
    return date;
  });

  function goToWeek(next: Date) {
    setWeekStart(next);
    setSelectedDay(next);
  }

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-3 shadow-sm">
      <div className="mb-3 flex items-center justify-between">
        <button
          type="button"
          onClick={() => goToWeek(addWeeks(weekStart, -1))}
          className="rounded-md px-2 py-1 text-sm text-slate-600 hover:bg-slate-100"
        >
          ← Anterior
        </button>
        <button
          type="button"
          onClick={() => goToWeek(startOfWeek(new Date(), { locale: pt }))}
          className="text-sm font-medium text-indigo-600 hover:underline"
        >
          Hoje
        </button>
        <button
          type="button"
          onClick={() => goToWeek(addWeeks(weekStart, 1))}
          className="rounded-md px-2 py-1 text-sm text-slate-600 hover:bg-slate-100"
        >
          Seguinte →
        </button>
      </div>

      <div className="grid grid-cols-7 gap-1">
        {days.map((day) => {
          const isSelected = isSameDay(day, selectedDay);
          const isToday = isSameDay(day, new Date());
          return (
            <button
              key={day.toISOString()}
              type="button"
              onClick={() => setSelectedDay(day)}
              style={custodyDayStyle(custodyPeriods, day, isSelected ? 0.45 : 0.25)}
              className={`flex flex-col items-center rounded-lg py-2 text-xs ${
                isSelected ? 'ring-2 ring-indigo-500' : ''
              } ${isToday ? 'font-bold' : ''}`}
            >
              <span className="text-slate-500">{format(day, 'EEEEEE', { locale: pt })}</span>
              <span className="text-sm text-slate-900">{format(day, 'd')}</span>
            </button>
          );
        })}
      </div>

      <div className="mt-4 border-t border-slate-100 pt-3">
        <div className="mb-2 flex items-center justify-between">
          <h3 className="text-sm font-semibold text-slate-900">
            {format(selectedDay, "EEEE, d 'de' MMMM", { locale: pt })}
          </h3>
          <button
            type="button"
            onClick={() => onSelectDay(selectedDay)}
            className="text-sm font-medium text-indigo-600 hover:underline"
          >
            + Evento
          </button>
        </div>

        <div className="space-y-2">
          {events
            .filter((e) => isSameDay(e.start, selectedDay))
            .sort((a, b) => a.start.getTime() - b.start.getTime())
            .map((event) => (
              <button
                key={event.eventId}
                type="button"
                onClick={() => onSelectEvent(event)}
                className="flex w-full items-center gap-3 rounded-md border border-slate-100 bg-slate-50 px-3 py-2 text-left hover:bg-slate-100"
              >
                <span className="text-xs font-medium text-slate-500">{format(event.start, 'HH:mm')}</span>
                <span className="text-sm text-slate-900">{event.title}</span>
              </button>
            ))}

          {events.filter((e) => isSameDay(e.start, selectedDay)).length === 0 && (
            <p className="text-sm text-slate-400">Sem eventos neste dia.</p>
          )}
        </div>
      </div>
    </div>
  );
}
