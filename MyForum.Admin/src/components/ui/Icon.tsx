import type { LucideIcon, LucideProps } from "lucide-react";
import {
  AlertTriangle,
  Ban,
  CalendarDays,
  Eye,
  EyeOff,
  Funnel,
  Hammer,
  LayoutDashboard,
  ListTree,
  Lock,
  LockOpen,
  LogOut,
  MessageSquareText,
  Pencil,
  Plus,
  RefreshCw,
  Rows3,
  Search,
  Shield,
  Trash2,
} from "lucide-react";

export type IconName =
  | "dashboard"
  | "boards"
  | "threads"
  | "posts"
  | "bans"
  | "logout"
  | "search"
  | "plus"
  | "edit"
  | "delete"
  | "eye"
  | "eyeOff"
  | "lock"
  | "unlock"
  | "refresh"
  | "filter"
  | "calendar"
  | "warning"
  | "hammer"
  | "shield";

interface IconProps extends LucideProps {
  name: IconName;
  size?: number;
}

const iconMap: Record<IconName, LucideIcon> = {
  dashboard: LayoutDashboard,
  boards: Rows3,
  threads: ListTree,
  posts: MessageSquareText,
  bans: Ban,
  logout: LogOut,
  search: Search,
  plus: Plus,
  edit: Pencil,
  delete: Trash2,
  eye: Eye,
  eyeOff: EyeOff,
  lock: Lock,
  unlock: LockOpen,
  refresh: RefreshCw,
  filter: Funnel,
  calendar: CalendarDays,
  warning: AlertTriangle,
  hammer: Hammer,
  shield: Shield,
};

export const Icon = ({ name, size = 18, ...props }: IconProps) => {
  const LucideComponent = iconMap[name];
  return <LucideComponent size={size} strokeWidth={1.8} aria-hidden {...props} />;
};
