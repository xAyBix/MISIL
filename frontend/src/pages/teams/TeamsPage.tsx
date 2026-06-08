import { useEffect, useState } from 'react'
import { useProject } from '@/hooks/useProject'
import { usePermissions } from '@/hooks/usePermissions'
import api from '@/lib/api'
import type { TeamDto } from '@/types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Users, Plus, X, Trash2, UserPlus, UserMinus, ChevronDown, ChevronRight } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { useToast } from '@/hooks/use-toast'

interface TeamMemberDetail {
  userId: string
  userName: string
  email: string
  displayName: string | null
  avatarUrl: string | null
  joinedAt: string
}

interface ProjectMember {
  userId: string
  userName: string
  email: string
}

const avatarColors = [
  'bg-misil-700', 'bg-blue-700', 'bg-amber-700', 'bg-purple-700',
  'bg-green-700', 'bg-rose-700', 'bg-cyan-700', 'bg-orange-700',
]

function getColor(id: string) {
  let hash = 0
  for (let i = 0; i < id.length; i++) hash = ((hash << 5) - hash) + id.charCodeAt(i)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

export function TeamsPage() {
  const { project } = useProject()
  const { can } = usePermissions()
  const { toast } = useToast()
  const [teams, setTeams] = useState<TeamDto[]>([])
  const [showDialog, setShowDialog] = useState(false)
  const [teamName, setTeamName] = useState('')
  const [teamDescription, setTeamDescription] = useState('')
  const [expanded, setExpanded] = useState<Record<string, TeamMemberDetail[]>>({})
  const [loadingMembers, setLoadingMembers] = useState<Record<string, boolean>>({})
  const [showAddMember, setShowAddMember] = useState<string | null>(null)
  const [projectMembers, setProjectMembers] = useState<ProjectMember[]>([])

  useEffect(() => {
    if (!project) return
    api.get<TeamDto[]>(`/projects/${project.id}/teams`).then((r) => setTeams(r.data))
  }, [project])

  if (!project) return null

  const toggleExpand = async (teamId: string) => {
    if (expanded[teamId]) {
      const { [teamId]: _, ...rest } = expanded
      setExpanded(rest)
      return
    }
    setLoadingMembers((prev) => ({ ...prev, [teamId]: true }))
    try {
      const res = await api.get<TeamMemberDetail[]>(`/projects/${project.id}/teams/${teamId}/members`)
      setExpanded((prev) => ({ ...prev, [teamId]: res.data }))
    } catch {
      toast({ title: 'Failed to load members', variant: 'destructive' })
    } finally {
      setLoadingMembers((prev) => ({ ...prev, [teamId]: false }))
    }
  }

  const handleCreate = async () => {
    if (!teamName) return
    try {
      const res = await api.post<TeamDto>(`/projects/${project.id}/teams`, { name: teamName, description: teamDescription || null })
      setTeams((prev) => [...prev, res.data])
      setShowDialog(false)
      setTeamName('')
      setTeamDescription('')
    } catch { /* ignore */ }
  }

  const handleRemoveMember = async (teamId: string, userId: string) => {
    try {
      await api.delete(`/projects/${project.id}/teams/${teamId}/members/${userId}`)
      setExpanded((prev) => ({
        ...prev,
        [teamId]: (prev[teamId] ?? []).filter((m) => m.userId !== userId),
      }))
      setTeams((prev) => prev.map((t) => t.id === teamId ? { ...t, memberCount: t.memberCount - 1 } : t))
      toast({ title: 'Member removed from team' })
    } catch {
      toast({ title: 'Failed to remove member', variant: 'destructive' })
    }
  }

  const openAddMember = async (teamId: string) => {
    setShowAddMember(teamId)
    try {
      const res = await api.get<ProjectMember[]>(`/projects/${project.id}/members`)
      setProjectMembers(res.data)
    } catch { /* ignore */ }
  }

  const handleAddMember = async (teamId: string, userId: string) => {
    try {
      await api.post(`/projects/${project.id}/teams/${teamId}/members/${userId}`)
      toast({ title: 'Member added to team' })
      // Reload members
      const res = await api.get<TeamMemberDetail[]>(`/projects/${project.id}/teams/${teamId}/members`)
      setExpanded((prev) => ({ ...prev, [teamId]: res.data }))
      setTeams((prev) => prev.map((t) => t.id === teamId ? { ...t, memberCount: t.memberCount + 1 } : t))
    } catch {
      toast({ title: 'Failed to add member', variant: 'destructive' })
    }
  }

  return (
    <>
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Users className="h-6 w-6 text-misil-500" />
            <div>
              <h1 className="text-2xl font-bold text-white">Teams</h1>
              <p className="text-sm text-gray-500">{project.name}</p>
            </div>
          </div>
          {can('manage_teams') && <Button onClick={() => setShowDialog(true)}><Plus className="h-4 w-4" /> New Team</Button>}
        </div>

        {teams.length === 0 ? (
          <div className="bg-surface-1 border border-surface-3 rounded-lg flex items-center justify-center h-64 text-gray-500 flex-col gap-3">
            <Users className="h-12 w-12" />
            <p className="text-sm">No teams in {project.name}</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {teams.map((t) => {
              const isExpanded = !!expanded[t.id]
              const members = expanded[t.id] ?? []
              const isLoading = loadingMembers[t.id]
              const existingIds = new Set(members.map((m) => m.userId))
              const available = projectMembers.filter((pm) => !existingIds.has(pm.userId))

              return (
                <div key={t.id} className="bg-surface-1 border border-surface-3 rounded-lg overflow-hidden">
                  {/* Header */}
                  <div
                    className="p-4 cursor-pointer hover:bg-white/[0.02] transition-colors"
                    onClick={() => toggleExpand(t.id)}
                  >
                    <div className="flex items-center justify-between mb-1">
                      <div className="flex items-center gap-2">
                        {isExpanded ? <ChevronDown className="h-4 w-4 text-gray-500" /> : <ChevronRight className="h-4 w-4 text-gray-500" />}
                        <h3 className="font-semibold text-white">{t.name}</h3>
                      </div>
                      <Badge variant="secondary">{t.memberCount} member{t.memberCount !== 1 ? 's' : ''}</Badge>
                    </div>
                    {t.description && <p className="text-sm text-gray-400 ml-6">{t.description}</p>}
                  </div>

                  {/* Members list */}
                  {isExpanded && (
                    <div className="border-t border-surface-3">
                      {isLoading ? (
                        <div className="p-4 text-center text-gray-500 text-sm">Loading...</div>
                      ) : (
                        <>
                          {members.length === 0 ? (
                            <div className="p-4 text-center text-gray-600 text-sm">No members</div>
                          ) : (
                            <div className="divide-y divide-surface-3">
                              {members.map((m) => {
                                const name = m.displayName || m.userName
                                return (
                                  <div key={m.userId} className="flex items-center gap-3 px-4 py-2">
                                    <div className={`w-7 h-7 rounded-full ${getColor(m.userId)} flex items-center justify-center text-[10px] text-white font-medium shrink-0`}>
                                      {name.charAt(0).toUpperCase()}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                      <p className="text-sm text-white truncate">{name}</p>
                                      <p className="text-[10px] text-gray-500 truncate">{m.email}</p>
                                    </div>
                                    {can('manage_teams') && (
                                      <button
                                        onClick={() => handleRemoveMember(t.id, m.userId)}
                                        className="p-1 text-gray-600 hover:text-red-400 transition-colors"
                                      >
                                        <UserMinus className="h-3.5 w-3.5" />
                                      </button>
                                    )}
                                  </div>
                                )
                              })}
                            </div>
                          )}
                          {can('manage_teams') && (
                            <div className="p-2 border-t border-surface-3">
                              <button
                                onClick={() => openAddMember(t.id)}
                                className="flex items-center gap-1.5 w-full px-3 py-1.5 rounded text-xs text-gray-400 hover:text-white hover:bg-surface-3 transition-colors"
                              >
                                <UserPlus className="h-3.5 w-3.5" />
                                Add member
                              </button>
                            </div>
                          )}
                        </>
                      )}
                    </div>
                  )}

                  {/* Add member popover */}
                  {showAddMember === t.id && (
                    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={() => setShowAddMember(null)}>
                      <div className="bg-surface-1 border border-surface-3 rounded-lg p-5 w-full max-w-sm space-y-3" onClick={(e) => e.stopPropagation()}>
                        <div className="flex items-center justify-between">
                          <h3 className="text-sm font-semibold text-white">Add Member to {t.name}</h3>
                          <button onClick={() => setShowAddMember(null)} className="text-gray-500 hover:text-white"><X className="h-4 w-4" /></button>
                        </div>
                        <div className="max-h-60 overflow-y-auto space-y-1">
                          {available.length === 0 ? (
                            <p className="text-sm text-gray-500 text-center py-6">No members available</p>
                          ) : (
                            available.map((pm) => (
                              <div key={pm.userId} className="flex items-center gap-3 px-3 py-2 rounded hover:bg-surface-2 text-sm">
                                <div className={`w-7 h-7 rounded-full ${getColor(pm.userId)} flex items-center justify-center text-[10px] text-white font-medium shrink-0`}>
                                  {pm.userName.charAt(0).toUpperCase()}
                                </div>
                                <div className="flex-1 min-w-0">
                                  <p className="text-sm text-white truncate">{pm.userName}</p>
                                  <p className="text-[10px] text-gray-500 truncate">{pm.email}</p>
                                </div>
                                <button
                                  onClick={() => {
                                    handleAddMember(t.id, pm.userId)
                                    setShowAddMember(null)
                                  }}
                                  className="p-1 text-misil-500 hover:text-misil-400 transition-colors"
                                >
                                  <UserPlus className="h-4 w-4" />
                                </button>
                              </div>
                            ))
                          )}
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Create team dialog */}
      {showDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={() => setShowDialog(false)}>
          <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 w-full max-w-sm space-y-4" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-white">New Team</h2>
              <button onClick={() => setShowDialog(false)} className="text-gray-500 hover:text-white"><X className="h-5 w-5" /></button>
            </div>
            <Input placeholder="Team Name" value={teamName} onChange={(e) => setTeamName(e.target.value)} />
            <Input placeholder="Description (optional)" value={teamDescription} onChange={(e) => setTeamDescription(e.target.value)} />
            <Button className="w-full" onClick={handleCreate} disabled={!teamName}>Create Team</Button>
          </div>
        </div>
      )}
    </>
  )
}
