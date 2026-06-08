import { useEffect, useRef, useState } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Bell, LogOut, PanelLeftClose, PanelLeftOpen, Settings } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import api from '@/lib/api'

interface Notification { id: string; message: string; action: string; isRead: boolean; createdAt: string; }

interface Props { onToggleSidebar: () => void }

export function TopBar({ onToggleSidebar }: Props) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [showInbox, setShowInbox] = useState(false)
  const [notifications, setNotifications] = useState<Notification[]>([])
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    api.get<Notification[]>('/notifications').then((r) => setNotifications(r.data)).catch(() => {})
  }, [])

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setShowInbox(false)
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  const unreadCount = notifications.filter((n) => !n.isRead).length

  const markAllRead = async () => {
    await api.post('/notifications/read-all')
    setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })))
  }

  return (
    <header className="h-13 bg-surface-1/80 backdrop-blur-xl border-b border-surface-3/60 flex items-center justify-between px-4 shrink-0">
      <div className="flex items-center gap-3">
        <button
          onClick={onToggleSidebar}
          className="p-1.5 rounded-lg text-gray-500 hover:text-white hover:bg-surface-3/50 transition-all duration-150"
          title="Toggle sidebar"
        >
          <PanelLeftClose className="h-4 w-4" />
        </button>
        <div className="h-5 w-px bg-gradient-to-b from-transparent via-surface-3 to-transparent" />
        <div className="flex items-center gap-2">
          <span className="text-sm text-gray-500">
            <span className="text-gray-500">Welcome back,</span>{' '}
            <span className="text-white font-medium">{user?.displayName ?? user?.username}</span>
          </span>
        </div>
      </div>
      <div className="flex items-center gap-1" ref={ref}>
        <div className="relative">
          <button
            onClick={() => setShowInbox(!showInbox)}
            className="p-2 rounded-lg text-gray-500 hover:text-white hover:bg-surface-3/50 transition-all duration-150 relative"
          >
            <Bell className="h-4 w-4" />
            {unreadCount > 0 && (
              <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-red-500 rounded-full ring-2 ring-surface-1 animate-pulse" />
            )}
          </button>
          {showInbox && (
            <div className="absolute right-0 top-full mt-1.5 w-80 bg-surface-1/80 backdrop-blur-xl border border-surface-3/60 rounded-xl shadow-2xl shadow-black/40 z-50 overflow-hidden">
              <div className="flex items-center justify-between px-4 py-3 border-b border-surface-3/50">
                <h3 className="text-sm font-semibold text-white">Notifications</h3>
                {unreadCount > 0 && (
                  <button onClick={markAllRead} className="text-[10px] text-misil-500 hover:text-misil-400 font-medium transition-colors">Mark all read</button>
                )}
              </div>
              <div className="max-h-80 overflow-y-auto">
                {notifications.length === 0 ? (
                  <div className="p-8 text-center text-gray-600 text-xs">No notifications</div>
                ) : (
                  notifications.map((n) => (
                    <div key={n.id} className={`px-4 py-3 border-b border-surface-3/30 last:border-0 transition-colors ${!n.isRead ? 'bg-misil-500/5' : 'hover:bg-surface-2/50'}`}>
                      <p className="text-sm text-white">{n.message}</p>
                      <p className="text-[10px] text-gray-600 mt-0.5">{new Date(n.createdAt).toLocaleDateString()}</p>
                    </div>
                  ))
                )}
              </div>
            </div>
          )}
        </div>
        <div className="h-5 w-px bg-gradient-to-b from-transparent via-surface-3 to-transparent mx-0.5" />
        <div className="relative group/profile">
          <button onClick={() => navigate('/settings')} className="p-1 rounded-lg hover:bg-surface-3/50 transition-all duration-150">
            <Avatar className="h-7 w-7 ring-2 ring-surface-3/50 hover:ring-misil-500/30 transition-all duration-200">
              <AvatarFallback className="bg-gradient-to-br from-misil-600 to-misil-800 text-white text-[11px] font-medium">
                {user?.displayName?.charAt(0)?.toUpperCase() || user?.username?.charAt(0)?.toUpperCase() || 'U'}
              </AvatarFallback>
            </Avatar>
          </button>
          <div className="absolute right-0 top-full mt-1.5 bg-surface-1/80 backdrop-blur-xl border border-surface-3/60 rounded-xl shadow-2xl shadow-black/40 py-1 min-w-[160px] opacity-0 invisible group-hover/profile:opacity-100 group-hover/profile:visible transition-all duration-150 z-50">
            <div className="px-3 py-2 border-b border-surface-3/30">
              <p className="text-sm font-medium text-white truncate">{user?.displayName || user?.username}</p>
              <p className="text-[10px] text-gray-600 truncate">{user?.email}</p>
            </div>
            <button onClick={() => navigate('/settings')} className="w-full flex items-center gap-2 px-3 py-1.5 text-sm text-gray-400 hover:text-white hover:bg-surface-3/50 transition-colors">
              <Settings className="h-3.5 w-3.5" />
              Settings
            </button>
            <button onClick={logout} className="w-full flex items-center gap-2 px-3 py-1.5 text-sm text-gray-400 hover:text-red-400 hover:bg-surface-3/50 transition-colors">
              <LogOut className="h-3.5 w-3.5" />
              Sign out
            </button>
          </div>
        </div>
        <button
          onClick={logout}
          className="p-2 rounded-lg text-gray-600 hover:text-red-400 hover:bg-surface-3/50 transition-all duration-150"
          title="Sign out"
        >
          <LogOut className="h-4 w-4" />
        </button>
      </div>
    </header>
  )
}
