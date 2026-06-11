import { Mail, Eye, CheckCircle2, Ban, Bell, type LucideIcon } from 'lucide-react';

interface TypeStyle {
  icon: LucideIcon;
  bg: string;
  fg: string;
  label: string;
}

const TYPE_CONFIG: Record<string, TypeStyle> = {
  NEW_REQUEST: {
    icon: Mail,
    bg: 'bg-blue-100',
    fg: 'text-blue-600',
    label: 'New request',
  },
  DATA_ACCESSED: {
    icon: Eye,
    bg: 'bg-amber-100',
    fg: 'text-amber-600',
    label: 'Data viewed',
  },
  REQUEST_REVIEWED: {
    icon: CheckCircle2,
    bg: 'bg-emerald-100',
    fg: 'text-emerald-600',
    label: 'Request reviewed',
  },
  ACCESS_REVOKED: {
    icon: Ban,
    bg: 'bg-red-100',
    fg: 'text-red-600',
    label: 'Access revoked',
  },
};

const FALLBACK_STYLE: TypeStyle = {
  icon: Bell,
  bg: 'bg-gray-100',
  fg: 'text-gray-600',
  label: '',
};

export function getTypeStyle(type: string): TypeStyle {
  return TYPE_CONFIG[type] ?? { ...FALLBACK_STYLE, label: type };
}

// Size variants must be full class strings so Tailwind picks them up at build time.
const SIZE_CLASS: Record<'sm' | 'md', { box: string; iconPx: number }> = {
  sm: { box: 'w-8 h-8', iconPx: 16 },
  md: { box: 'w-9 h-9', iconPx: 18 },
};

export function NotificationIcon({
  type,
  size = 'md',
}: {
  type: string;
  size?: 'sm' | 'md';
}) {
  const style = getTypeStyle(type);
  const { box, iconPx } = SIZE_CLASS[size];
  const Icon = style.icon;
  return (
    <div
      className={`flex-shrink-0 ${box} rounded-full flex items-center justify-center ${style.bg}`}
    >
      <Icon size={iconPx} className={style.fg} />
    </div>
  );
}

// Short relative time: "just now", "3m ago", "2h ago", "1d ago", or absolute date for older
export function formatRelativeTime(iso: string): string {
  const then = new Date(iso).getTime();
  const now = Date.now();
  const diffSec = Math.max(0, Math.floor((now - then) / 1000));

  if (diffSec < 60) return 'just now';
  const diffMin = Math.floor(diffSec / 60);
  if (diffMin < 60) return `${diffMin}m ago`;
  const diffHour = Math.floor(diffMin / 60);
  if (diffHour < 24) return `${diffHour}h ago`;
  const diffDay = Math.floor(diffHour / 24);
  if (diffDay < 7) return `${diffDay}d ago`;
  // older than a week — fall back to absolute compact date
  return new Date(iso).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}
