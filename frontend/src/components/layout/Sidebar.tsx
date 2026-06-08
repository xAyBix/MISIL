import { NavLink } from 'react-router-dom'
import { cn } from '@/lib/utils'
import { useProject } from '@/hooks/useProject'
import {
  LayoutDashboard, ListTodo, Kanban, Timer, MessageSquare,
  GitBranch, Users, ScrollText, Settings, Rocket, ArrowLeft,
  BarChart3, Shield, UserPlus, Home,
} from 'lucide-react'

const globalNavItems = [
  { to: '/projects', label: 'Projects', icon: Home },
  { to: '/settings', label: 'Settings', icon: Settings },
]

const projectNavItems = [
  { to: '/board', label: 'Board', icon: Kanban },
  { to: '/backlog', label: 'Backlog', icon: ListTodo },
  { to: '/sprint', label: 'Sprint', icon: Timer },
  { to: '/chat', label: 'Chat', icon: MessageSquare },
  { to: '/gantt', label: 'Gantt', icon: GitBranch },
  { to: '/teams', label: 'Teams', icon: Users },
  { to: '/stats', label: 'Stats', icon: BarChart3 },
  { to: '/members', label: 'Members', icon: UserPlus },
  { to: '/roles', label: 'Roles', icon: Shield },
  { to: '/logs', label: 'Activity Log', icon: ScrollText },
]

interface Props { open: boolean }

export function Sidebar({ open }: Props) {
  const { project, clearProject } = useProject()

  return (
    <aside
      className="bg-surface-1/80 backdrop-blur-xl border-r border-surface-3/60 flex flex-col h-full overflow-hidden transition-all duration-300 ease-out shrink-0"
      style={{ width: open ? 240 : 0 }}
    >
      {/* Logo */}
      <div className="flex items-center gap-2.5 px-4 h-13 border-b border-surface-3/50 shrink-0" style={{ minWidth: 240 }}>
        <div className="w-8 h-8 rounded-xl bg-gradient-to-br from-misil-500 to-misil-700 flex items-center justify-center shadow-lg shadow-misil-600/20">
          <Rocket className="h-4 w-4 text-white" />
        </div>
        <div>
          <span className="font-bold text-white text-base tracking-tight">MISIL</span>
          <span className="text-[10px] text-misil-500 block -mt-0.5 font-medium">project management</span>
        </div>
      </div>

      {/* Project context */}
      {project && (
        <div className="px-3 pt-3 pb-2 border-b border-surface-3/40 shrink-0" style={{ minWidth: 240 }}>
          <div className="flex items-center gap-2.5 mb-1.5">
            <button onClick={clearProject} className="p-1 rounded-lg hover:bg-surface-3/50 text-gray-500 hover:text-white transition-colors shrink-0">
              <ArrowLeft className="h-4 w-4" />
            </button>
            <div className="w-7 h-7 rounded-lg bg-gradient-to-br from-misil-600 to-misil-800 flex items-center justify-center shrink-0 shadow-sm">
              <span className="text-[10px] font-bold text-white">{project.name.charAt(0).toUpperCase()}</span>
            </div>
            <div className="min-w-0">
              <span className="text-sm font-medium text-white truncate block">{project.name}</span>
              <span className="text-[10px] text-misil-500 font-medium">{project.key}</span>
            </div>
          </div>
        </div>
      )}

      {/* Navigation */}
      <nav className="flex-1 p-2 space-y-0.5 overflow-y-auto overflow-x-hidden" style={{ minWidth: 240 }}>
        <div className="px-3 pb-1 pt-2">
          <span className="text-[9px] uppercase tracking-widest text-gray-600 font-semibold">General</span>
        </div>
        {globalNavItems.map(({ to, label, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/projects'}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-all duration-150',
                isActive
                  ? 'bg-misil-500/10 text-misil-400 font-medium border border-misil-500/20 shadow-sm'
                  : 'text-gray-400 hover:text-white hover:bg-surface-3/40 border border-transparent',
              )
            }
          >
            <Icon className="h-4 w-4 shrink-0" />
            <span className="whitespace-nowrap">{label}</span>
          </NavLink>
        ))}

        {project && (
          <>
            <div className="mt-4 mb-1 mx-3 pt-3 border-t border-surface-3/40">
              <span className="text-[9px] uppercase tracking-widest text-gray-600 font-semibold">Project</span>
            </div>
            {projectNavItems.map(({ to, label, icon: Icon }) => (
              <NavLink
                key={to}
                to={to}
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-all duration-150',
                    isActive
                      ? 'bg-misil-500/10 text-misil-400 font-medium border border-misil-500/20 shadow-sm'
                      : 'text-gray-400 hover:text-white hover:bg-surface-3/40 border border-transparent',
                  )
                }
              >
                <Icon className="h-4 w-4 shrink-0" />
                <span className="whitespace-nowrap">{label}</span>
              </NavLink>
            ))}
          </>
        )}
      </nav>
    </aside>
  )
}
