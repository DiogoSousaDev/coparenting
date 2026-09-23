export type CustodyRole = 0 | 1; // 0 = Pai, 1 = Mãe (mesma numeração do backend)

export const custodyRoleLabel = (role: CustodyRole) => (role === 0 ? 'Pai' : 'Mãe');

export interface CustodySegment {
  role: CustodyRole;
  durationHours: number;
}

export interface CustodySchedule {
  anchorStart: string;
  paiColor: string;
  maeColor: string;
  segments: CustodySegment[];
}

export interface CustodyPeriod {
  start: Date;
  end: Date;
  role: CustodyRole;
  color: string;
}

interface CustodyPeriodDto {
  start: string;
  end: string;
  role: CustodyRole;
  color: string;
}

export function parseCustodyPeriods(dtos: CustodyPeriodDto[]): CustodyPeriod[] {
  return dtos.map((p) => ({ start: new Date(p.start), end: new Date(p.end), role: p.role, color: p.color }));
}

export function colorForInstant(periods: CustodyPeriod[], instant: Date): string | null {
  const period = periods.find((p) => p.start <= instant && instant < p.end);
  return period?.color ?? null;
}

export function hexToRgba(hex: string, alpha: number): string {
  const normalized = hex.replace('#', '');
  const r = parseInt(normalized.substring(0, 2), 16);
  const g = parseInt(normalized.substring(2, 4), 16);
  const b = parseInt(normalized.substring(4, 6), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

// Cor de fundo de um dia inteiro, a partir da guarda em vigor ao início e ao fim desse dia.
// Se houver troca nesse dia, devolve um gradiente diagonal com as duas cores.
export function custodyDayStyle(
  periods: CustodyPeriod[],
  day: Date,
  alpha = 0.2,
): { backgroundColor?: string; backgroundImage?: string } {
  const startOfDay = new Date(day.getFullYear(), day.getMonth(), day.getDate(), 0, 0, 0);
  const endOfDay = new Date(day.getFullYear(), day.getMonth(), day.getDate(), 23, 59, 59, 999);
  const startColor = colorForInstant(periods, startOfDay);
  const endColor = colorForInstant(periods, endOfDay);

  if (!startColor && !endColor) {
    return {};
  }
  if (startColor === endColor) {
    return { backgroundColor: hexToRgba(startColor!, alpha) };
  }

  return {
    backgroundImage: `linear-gradient(135deg, ${hexToRgba(startColor ?? endColor!, alpha)} 50%, ${hexToRgba(endColor ?? startColor!, alpha)} 50%)`,
  };
}

export function totalCycleHours(segments: CustodySegment[]): number {
  return segments.reduce((sum, s) => sum + s.durationHours, 0);
}

export function formatCycleDuration(totalHours: number): string {
  const days = Math.floor(totalHours / 24);
  const hours = totalHours % 24;
  if (days === 0) return `${hours}h`;
  if (hours === 0) return `${days} dia${days === 1 ? '' : 's'}`;
  return `${days} dia${days === 1 ? '' : 's'} ${hours}h`;
}
