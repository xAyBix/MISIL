import { useState } from 'react'
import { useAuth } from '@/hooks/useAuth'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { useToast } from '@/hooks/use-toast'
import api from '@/lib/api'

export function SettingsPage() {
  const { user, updateUser } = useAuth()
  const { toast } = useToast()
  const [displayName, setDisplayName] = useState(user?.displayName ?? '')
  const [saving, setSaving] = useState(false)
  const [theme, setTheme] = useState(() => localStorage.getItem('theme') || 'dark')

  const handleSave = async () => {
    if (!displayName.trim()) return
    setSaving(true)
    try {
      const res = await api.put('/auth/profile', { displayName: displayName.trim() })
      updateUser(res.data)
      toast({ title: 'Profile updated' })
    } catch {
      toast({ title: 'Failed to update profile', variant: 'destructive' })
    } finally { setSaving(false) }
  }

  const handleThemeChange = (t: string) => {
    setTheme(t)
    localStorage.setItem('theme', t)
    document.documentElement.classList.toggle('dark', t === 'dark')
    document.documentElement.classList.toggle('light', t === 'light')
  }

  if (!user) return null

  return (
    <div className="max-w-2xl space-y-6">
      <h1 className="text-2xl font-bold text-white">Settings</h1>

      {/* Profile */}
      <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 space-y-4">
        <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wider">Profile</h2>
        <div className="flex items-center gap-4">
          <Avatar className="h-16 w-16">
            <AvatarFallback className="bg-misil-700 text-white text-xl">
              {displayName?.charAt(0)?.toUpperCase() || user.username?.charAt(0)?.toUpperCase() || 'U'}
            </AvatarFallback>
          </Avatar>
          <div>
            <h2 className="text-lg font-semibold text-white">{displayName || user.username}</h2>
            <p className="text-sm text-gray-400">@{user.username}</p>
            <p className="text-sm text-gray-500">{user.email}</p>
          </div>
        </div>
        <Separator />
        <div className="space-y-3">
          <div>
            <label className="text-xs text-gray-500 mb-1 block">Display Name</label>
            <Input value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
          </div>
          <Button onClick={handleSave} disabled={saving || !displayName.trim()}>{saving ? 'Saving...' : 'Save'}</Button>
          {displayName !== (user.displayName ?? '') && (
            <p className="text-xs text-amber-400">Unsaved changes</p>
          )}
        </div>
      </div>

      {/* Preferences */}
      <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 space-y-4">
        <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wider">Preferences</h2>
        <div className="space-y-3">
          <div className="flex justify-between items-center">
            <span className="text-sm text-gray-400">Theme</span>
            <select value={theme} onChange={(e) => handleThemeChange(e.target.value)}
              className="bg-surface-2 border border-surface-3 rounded px-3 py-1.5 text-sm text-white outline-none focus:border-misil-500">
              <option value="dark">Dark</option>
              <option value="light">Light</option>
            </select>
          </div>
        </div>
      </div>

      {/* Account Info */}
      <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 space-y-4">
        <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wider">Account</h2>
        <div className="space-y-2 text-sm">
          <div className="flex justify-between"><span className="text-gray-400">Email</span><span className="text-white">{user.email}</span></div>
          <div className="flex justify-between"><span className="text-gray-400">Username</span><span className="text-white">@{user.username}</span></div>
          <div className="flex justify-between"><span className="text-gray-400">Role</span><Badge variant="secondary">Member</Badge></div>
        </div>
      </div>
    </div>
  )
}
