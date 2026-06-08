import { useEffect, useState } from 'react'
import { useProject } from '@/hooks/useProject'
import { usePermissions } from '@/hooks/usePermissions'
import api from '@/lib/api'
import type { Role } from '@/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Users, Plus, X, Trash2, Calendar, Shield, Mail } from 'lucide-react'
import { useToast } from '@/hooks/use-toast'

interface ProjectMemberDto {
  userId: string
  userName: string
  email: string
  roleId: string
  displayName: string
  avatarUrl: string | null
  joinedAt: string
}

const avatarColors = [
  'bg-misil-700', 'bg-blue-700', 'bg-amber-700', 'bg-purple-700',
  'bg-green-700', 'bg-rose-700', 'bg-cyan-700', 'bg-orange-700',
]

function getAvatarColor(userId: string) {
  let hash = 0
  for (let i = 0; i < userId.length; i++) hash = ((hash << 5) - hash) + userId.charCodeAt(i)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

export function MembersPage() {
  const { project } = useProject()
  const { can } = usePermissions()
  const { toast } = useToast()
  const [members, setMembers] = useState<ProjectMemberDto[]>([])
  const [roles, setRoles] = useState<Role[]>([])
  const [showInvite, setShowInvite] = useState(false)
  const [inviteEmail, setInviteEmail] = useState('')
  const [inviteRole, setInviteRole] = useState('')

  useEffect(() => {
    if (!project) return
    api.get<ProjectMemberDto[]>(`/projects/${project.id}/members`).then((r) => setMembers(r.data))
    api.get<Role[]>(`/projects/${project.id}/roles`).then((r) => {
      setRoles(r.data)
      if (r.data.length > 0) setInviteRole(r.data[0].id)
    })
  }, [project])

  if (!project) return null

  const handleInvite = async () => {
    if (!inviteEmail || !inviteRole) return
    try {
      await api.post(`/projects/${project.id}/members`, { email: inviteEmail, roleId: inviteRole })
      toast({ title: 'Member invited' })
      setShowInvite(false)
      setInviteEmail('')
      const res = await api.get<ProjectMemberDto[]>(`/projects/${project.id}/members`)
      setMembers(res.data)
    } catch {
      toast({ title: 'Failed to invite member', variant: 'destructive' })
    }
  }

  const handleRemove = async (userId: string) => {
    try {
      await api.delete(`/projects/${project.id}/members/${userId}`)
      setMembers((prev) => prev.filter((m) => m.userId !== userId))
      toast({ title: 'Member removed' })
    } catch {
      toast({ title: 'Failed to remove member', variant: 'destructive' })
    }
  }

  const handleRoleChange = async (userId: string, roleId: string) => {
    try {
      await api.patch(`/projects/${project.id}/members/${userId}/role`, roleId, {
        headers: { 'Content-Type': 'application/json' },
      })
      setMembers((prev) => prev.map((m) => (m.userId === userId ? { ...m, roleId } : m)))
      toast({ title: 'Role updated' })
    } catch {
      toast({ title: 'Failed to update role', variant: 'destructive' })
    }
  }

  return (
    <>
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Users className="h-6 w-6 text-misil-500" />
            <div>
              <h1 className="text-2xl font-bold text-white">Members</h1>
              <p className="text-sm text-gray-500">{project.name} &middot; {members.length} member{members.length !== 1 ? 's' : ''}</p>
            </div>
          </div>
          {can('invite_members') && <Button onClick={() => setShowInvite(true)}><Plus className="h-4 w-4" /> Invite Member</Button>}
        </div>

        <div className="bg-surface-1 border border-surface-3 rounded-lg divide-y divide-surface-3">
          {members.length === 0 ? (
            <div className="p-8 text-center text-gray-500">No members</div>
          ) : (
            members.map((m) => {
              const role = roles.find((r) => r.id === m.roleId)
              const isOwner = role?.name === 'Owner'
              const displayName = m.displayName || m.userName
              return (
                <div key={m.userId} className="flex items-center gap-4 px-4 py-3 hover:bg-white/[0.02] transition-colors">
                  <div className={`w-9 h-9 rounded-full ${getAvatarColor(m.userId)} flex items-center justify-center text-sm text-white font-medium shrink-0`}>
                    {displayName.charAt(0).toUpperCase()}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <p className="text-sm text-white font-medium truncate">{displayName}</p>
                      {isOwner && <Badge variant="outline" className="text-[9px] text-amber-500 border-amber-600/30 bg-amber-600/10 px-1">Owner</Badge>}
                    </div>
                    <div className="flex items-center gap-3 text-[11px] text-gray-500 mt-0.5">
                      <span className="flex items-center gap-1"><Mail className="h-3 w-3" /> {m.email}</span>
                      {m.joinedAt && (
                        <span className="flex items-center gap-1"><Calendar className="h-3 w-3" /> {new Date(m.joinedAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}</span>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Shield className="h-3.5 w-3.5 text-gray-600 shrink-0" />
                    {can('invite_members') ? (
                      <select
                        className="bg-surface-2 border border-surface-3 rounded px-2 py-1 text-xs text-white outline-none focus:border-misil-500"
                        value={m.roleId}
                        onChange={(e) => handleRoleChange(m.userId, e.target.value)}
                      >
                        {roles.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
                      </select>
                    ) : (
                      <span className="text-xs text-gray-400">{role?.name ?? 'Unknown'}</span>
                    )}
                    {!isOwner && can('remove_members') && (
                      <button onClick={() => handleRemove(m.userId)} className="p-1.5 text-gray-500 hover:text-red-400 hover:bg-surface-3 rounded transition-colors">
                        <Trash2 className="h-3.5 w-3.5" />
                      </button>
                    )}
                  </div>
                </div>
              )
            })
          )}
        </div>
      </div>

      {showInvite && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={() => setShowInvite(false)}>
          <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 w-full max-w-sm space-y-4" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-white">Invite Member</h2>
              <button onClick={() => setShowInvite(false)} className="text-gray-500 hover:text-white"><X className="h-5 w-5" /></button>
            </div>
            <div className="space-y-1">
              <label className="text-xs text-gray-500">Email address (User ID)</label>
              <Input placeholder="user@example.com" value={inviteEmail} onChange={(e) => setInviteEmail(e.target.value)} />
            </div>
            <div className="space-y-1">
              <label className="text-xs text-gray-500">Role</label>
              <select
                className="w-full bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500"
                value={inviteRole}
                onChange={(e) => setInviteRole(e.target.value)}
              >
                {roles.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
              </select>
            </div>
            <Button className="w-full" onClick={handleInvite} disabled={!inviteEmail || !inviteRole}>Send Invitation</Button>
          </div>
        </div>
      )}
    </>
  )
}
