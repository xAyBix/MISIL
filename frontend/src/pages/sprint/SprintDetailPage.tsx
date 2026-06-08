import { useEffect, useState, useCallback } from 'react'
import { useProject } from '@/hooks/useProject'
import { usePermissions } from '@/hooks/usePermissions'
import { useSignalR } from '@/hooks/useSignalR'
import api from '@/lib/api'
import type { IssueDto, SprintDto } from '@/types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { toast } from '@/hooks/use-toast'
import { X, Timer, Play, Plus, Link2, User, Users, CheckCircle2, Target } from 'lucide-react'
import { IssueDetailPanel } from '@/components/shared/IssueDetailPanel'

export function SprintDetailPage() {
  const { project } = useProject()
  const { can } = usePermissions()
  const { joinProject, on } = useSignalR()
  const [sprints, setSprints] = useState<SprintDto[]>([])
  const [busyId, setBusyId] = useState<string | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const [sprintForm, setSprintForm] = useState({ name: '', goal: '', startDate: '', endDate: '' })
  const [creating, setCreating] = useState(false)
  const [sprintIssues, setSprintIssues] = useState<IssueDto[]>([])
  const [showAddIssue, setShowAddIssue] = useState(false)
  const [availableIssues, setAvailableIssues] = useState<IssueDto[]>([])
  const [addingIssueId, setAddingIssueId] = useState<string | null>(null)
  const [selectedIssueId, setSelectedIssueId] = useState<string | null>(null)
  const [memberMap, setMemberMap] = useState<Record<string, string>>({})
  const [teamMap, setTeamMap] = useState<Record<string, string>>({})

  const activeSprint = sprints.find((s) => s.isActive && !s.isCompleted)
  const pastSprints = sprints.filter((s) => s.isCompleted)
  const plannedSprints = sprints.filter((s) => !s.isActive && !s.isCompleted)

  const loadSprints = useCallback(async (pid: string) => {
    const r = await api.get<SprintDto[]>(`/projects/${pid}/sprints`)
    setSprints(r.data)
  }, [])

  const loadSprintIssues = useCallback(async (pid: string, sid: string) => {
    const r = await api.get(`/projects/${pid}/issues?sprintId=${sid}`)
    setSprintIssues(Array.isArray(r.data) ? r.data : r.data.items ?? [])
  }, [])

  useEffect(() => {
    if (!project) return
    loadSprints(project.id)
    api.get<{ userId: string; userName: string }[]>(`/projects/${project.id}/members`).then((r) => {
      const map: Record<string, string> = {}
      r.data.forEach((m) => { map[m.userId] = m.userName })
      setMemberMap(map)
    })
    api.get<{ id: string; name: string }[]>(`/projects/${project.id}/teams`).then((r) => {
      const map: Record<string, string> = {}
      r.data.forEach((t) => { map[t.id] = t.name })
      setTeamMap(map)
    })
    joinProject(project.id)
  }, [project])

  useEffect(() => {
    if (!project || !activeSprint) { setSprintIssues([]); return }
    loadSprintIssues(project.id, activeSprint.id)
  }, [project, activeSprint])

  useEffect(() => {
    if (!project) return
    const unsub1 = on('IssueCreated', () => { loadSprintIssues(project.id, activeSprint?.id ?? ''); loadSprints(project.id) })
    const unsub2 = on('IssueUpdated', () => { loadSprintIssues(project.id, activeSprint?.id ?? ''); loadSprints(project.id) })
    const unsub3 = on('IssueDeleted', () => { loadSprintIssues(project.id, activeSprint?.id ?? ''); loadSprints(project.id) })
    return () => { unsub1(); unsub2(); unsub3() }
  }, [project, activeSprint])

  useEffect(() => {
    if (!showAddIssue || !project || !activeSprint) return
    api.get(`/projects/${project.id}/issues`)
      .then((r) => {
        const allIssues: IssueDto[] = Array.isArray(r.data) ? r.data : r.data.items ?? []
        const inSprintIds = new Set(sprintIssues.map((i) => i.id))
        setAvailableIssues(allIssues.filter((i) => !inSprintIds.has(i.id)))
      })
  }, [showAddIssue, project, activeSprint, sprintIssues])

  const handleStart = async (id: string) => {
    setBusyId(id)
    try {
      await api.post(`/projects/${project!.id}/sprints/${id}/start`)
      setSprints((prev) => prev.map((s) => s.id === id ? { ...s, isActive: true, startDate: new Date().toISOString() } : s))
    } finally { setBusyId(null) }
  }

  if (!project) return null

  const handleCreateSprint = async () => {
    if (!sprintForm.name) return
    setCreating(true)
    try {
      const res = await api.post<SprintDto>(`/projects/${project.id}/sprints`, {
        name: sprintForm.name, goal: sprintForm.goal || null,
        startDate: sprintForm.startDate || null, endDate: sprintForm.endDate || null,
      })
      setSprints((prev) => [...prev, res.data])
      setShowCreate(false)
      setSprintForm({ name: '', goal: '', startDate: '', endDate: '' })
    } catch (e) {
      const msg = (e as any)?.response?.data?.error || (e as Error)?.message || 'Unknown error'
      toast({ title: 'Failed to create sprint', description: msg, variant: 'destructive' })
    } finally { setCreating(false) }
  }

  const pct = activeSprint && activeSprint.totalStoryPoints > 0
    ? Math.round((activeSprint.completedStoryPoints / activeSprint.totalStoryPoints) * 100)
    : 0

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Timer className="h-6 w-6 text-misil-500" />
          <div>
            <h1 className="text-2xl font-bold text-white">Sprints</h1>
            <p className="text-sm text-gray-500">{project.name}</p>
          </div>
        </div>
        {can('manage_sprints') && <Button onClick={() => setShowCreate(true)}><Plus className="h-4 w-4" /> New Sprint</Button>}
      </div>

      {activeSprint ? (
        <div className="bg-surface-1 border border-misil-600/30 rounded-xl overflow-hidden">
          {/* Header */}
          <div className="p-5 border-b border-surface-3">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center gap-3">
                <div className="w-8 h-8 rounded-lg bg-misil-600/20 flex items-center justify-center">
                  <Target className="h-4 w-4 text-misil-400" />
                </div>
                <div>
                  <h2 className="font-semibold text-white text-lg">{activeSprint.name}</h2>
                  {activeSprint.goal && <p className="text-sm text-gray-400 mt-0.5">{activeSprint.goal}</p>}
                </div>
              </div>
              <Badge className="bg-misil-600/20 text-misil-400 border-misil-600/30">Active</Badge>
            </div>
            <div className="flex items-center gap-6 text-sm text-gray-500">
              <span className="flex items-center gap-1.5"><Timer className="h-3.5 w-3.5" /> {activeSprint.issueCount} issues</span>
              {activeSprint.endDate && (
                <span className="flex items-center gap-1.5">
                  Ends {new Date(activeSprint.endDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
                </span>
              )}
            </div>
          </div>

          {/* Progress bar */}
          {activeSprint.totalStoryPoints > 0 && (
            <div className="px-5 py-3 border-b border-surface-3">
              <div className="flex items-center justify-between mb-1.5">
                <span className="text-xs text-gray-400 font-medium">Story Points</span>
                <span className="text-xs text-gray-400">
                  <span className="text-misil-400 font-semibold">{activeSprint.completedStoryPoints}</span>
                  /{activeSprint.totalStoryPoints} ({pct}%)
                </span>
              </div>
              <div className="w-full h-2 bg-surface-3 rounded-full overflow-hidden">
                <div className="h-full bg-misil-500 rounded-full transition-all duration-500" style={{ width: `${pct}%` }} />
              </div>
            </div>
          )}

          {/* Issues */}
          <div className="p-5">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-sm font-medium text-gray-400 uppercase tracking-wider">Issues ({sprintIssues.length})</h3>
              {can('manage_sprints') && (
                <Button size="sm" variant="outline" onClick={() => setShowAddIssue(true)}>
                  <Plus className="h-3.5 w-3.5" /> Add Issue
                </Button>
              )}
            </div>
            {sprintIssues.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-10 text-gray-600">
                <CheckCircle2 className="h-8 w-8 mb-2" />
                <p className="text-sm">No issues in this sprint</p>
                {can('manage_sprints') && (
                  <Button size="sm" variant="outline" className="mt-3" onClick={() => setShowAddIssue(true)}>
                    <Plus className="h-3.5 w-3.5" /> Add issues from backlog
                  </Button>
                )}
              </div>
            ) : (
              <div className="space-y-1">
                {sprintIssues.map((issue) => (
                  <div key={issue.id} onClick={() => setSelectedIssueId(issue.id)}
                    className="flex items-center gap-3 px-3 py-2.5 rounded-lg hover:bg-surface-2 cursor-pointer group transition-colors">
                    <span className="text-xs w-4 shrink-0">{issue.issueType === 'Bug' ? '🔴' : issue.issueType === 'Story' ? '🟣' : issue.issueType === 'Epic' ? '🟠' : '🟢'}</span>
                    <span className="flex-1 text-sm text-white truncate">{issue.title}</span>
                    <span className="text-xs text-gray-500">{issue.storyPoints ?? '-'} pts</span>
                    <span className="text-[10px] text-gray-500 flex items-center gap-1 min-w-0">
                      {issue.assigneeId ? <><User className="h-3 w-3 shrink-0" /><span className="truncate">{memberMap[issue.assigneeId] || '...'}</span></>
                        : issue.assigneeTeamId ? <><Users className="h-3 w-3 shrink-0" /><span className="truncate">{teamMap[issue.assigneeTeamId] || '...'}</span></>
                        : null}
                    </span>
                    <Badge variant="outline" className="text-[10px] shrink-0">{issue.status === 'Done' ? '✅' : ''} {issue.status}</Badge>
                    {can('manage_sprints') && (
                      <button
                        onClick={async (e) => { e.stopPropagation()
                          try {
                            await api.delete(`/projects/${project!.id}/sprints/${activeSprint.id}/issues/${issue.id}`)
                            setSprintIssues((prev) => prev.filter((i) => i.id !== issue.id))
                            toast({ title: 'Issue removed from sprint' })
                          } catch { toast({ title: 'Failed to remove issue', variant: 'destructive' }) }
                        }}
                        className="p-1 rounded text-gray-600 hover:text-red-400 opacity-0 group-hover:opacity-100 transition-all shrink-0"
                      >
                        <X className="h-3.5 w-3.5" />
                      </button>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      ) : (
        <div className="bg-surface-1 border border-surface-3 rounded-xl p-10 text-center">
          <Timer className="h-10 w-10 text-gray-600 mx-auto mb-3" />
          <p className="text-gray-400 font-medium mb-1">No active sprint</p>
          <p className="text-sm text-gray-600 mb-4">Start a planned sprint to begin tracking work</p>
          {plannedSprints.length > 0 && (
            <Button onClick={() => handleStart(plannedSprints[0].id)} disabled={busyId !== null}>
              <Play className="h-4 w-4" /> Start {plannedSprints[0].name}
            </Button>
          )}
          {plannedSprints.length === 0 && can('manage_sprints') && (
            <Button onClick={() => setShowCreate(true)}><Plus className="h-4 w-4" /> Create Sprint</Button>
          )}
        </div>
      )}

      {/* Planned sprints */}
      {plannedSprints.length > 0 && (
        <div className="space-y-2">
          <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider">Planned Sprints</h3>
          <div className="grid gap-2">
            {plannedSprints.map((s) => (
              <div key={s.id} className="bg-surface-1 border border-surface-3 rounded-lg p-4 flex items-center justify-between group hover:border-surface-4 transition-colors">
                <div>
                  <span className="text-sm font-medium text-white">{s.name}</span>
                  {s.goal && <p className="text-xs text-gray-500 mt-0.5">{s.goal}</p>}
                  <span className="text-xs text-gray-600 mt-1 block">{s.issueCount} issues{s.totalStoryPoints > 0 ? ` · ${s.totalStoryPoints} pts` : ''}</span>
                </div>
                {can('manage_sprints') && (
                  <Button size="sm" onClick={() => handleStart(s.id)} disabled={busyId !== null}>
                    <Play className="h-3.5 w-3.5" /> Start
                  </Button>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Past sprints */}
      {pastSprints.length > 0 && (
        <div className="space-y-2">
          <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider">Past Sprints</h3>
          <div className="grid gap-2">
            {pastSprints.map((s) => {
              const pctDone = s.totalStoryPoints > 0 ? Math.round((s.completedStoryPoints / s.totalStoryPoints) * 100) : 0
              return (
                <div key={s.id} className="bg-surface-1 border border-surface-3 rounded-lg p-4">
                  <div className="flex items-center justify-between mb-2">
                    <div>
                      <span className="text-sm font-medium text-white">{s.name}</span>
                      <span className="text-xs text-gray-600 ml-2">{s.issueCount} issues</span>
                    </div>
                    <Badge variant="secondary">Completed</Badge>
                  </div>
                  {s.totalStoryPoints > 0 && (
                    <div className="flex items-center gap-2">
                      <div className="flex-1 h-1.5 bg-surface-3 rounded-full overflow-hidden">
                        <div className="h-full bg-misil-500 rounded-full" style={{ width: `${pctDone}%` }} />
                      </div>
                      <span className="text-xs text-gray-500 shrink-0">
                        {s.completedStoryPoints}/{s.totalStoryPoints} pts
                      </span>
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        </div>
      )}

      {showCreate && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={() => setShowCreate(false)}>
          <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 w-full max-w-sm space-y-4" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-white">New Sprint</h2>
              <button onClick={() => setShowCreate(false)} className="text-gray-500 hover:text-white"><X className="h-5 w-5" /></button>
            </div>
            <Input placeholder="Sprint Name *" value={sprintForm.name} onChange={(e) => setSprintForm({ ...sprintForm, name: e.target.value })} />
            <textarea
              placeholder="Goal (optional)"
              className="w-full bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500 resize-none h-20"
              value={sprintForm.goal}
              onChange={(e) => setSprintForm({ ...sprintForm, goal: e.target.value })}
            />
            <div className="grid grid-cols-2 gap-3">
              <Input type="date" placeholder="Start date" value={sprintForm.startDate} onChange={(e) => setSprintForm({ ...sprintForm, startDate: e.target.value })} />
              <Input type="date" placeholder="End date" value={sprintForm.endDate} onChange={(e) => setSprintForm({ ...sprintForm, endDate: e.target.value })} />
            </div>
            <Button className="w-full" onClick={handleCreateSprint} disabled={creating || !sprintForm.name}>
              {creating ? 'Creating...' : 'Create Sprint'}
            </Button>
          </div>
        </div>
      )}

      {showAddIssue && activeSprint && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={() => setShowAddIssue(false)}>
          <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 w-full max-w-md space-y-4" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-white">Add Issue to {activeSprint.name}</h2>
              <button onClick={() => setShowAddIssue(false)} className="text-gray-500 hover:text-white"><X className="h-5 w-5" /></button>
            </div>
            <div className="max-h-72 overflow-y-auto space-y-1">
              {availableIssues.length === 0 ? (
                <p className="text-sm text-gray-500 text-center py-8">No issues available to add</p>
              ) : (
                availableIssues.map((issue) => (
                  <div key={issue.id} className="flex items-center gap-2 px-3 py-2 rounded hover:bg-surface-2 text-sm">
                    <span className="text-xs">{issue.issueType === 'Bug' ? '🔴' : issue.issueType === 'Story' ? '🟣' : issue.issueType === 'Epic' ? '🟠' : '🟢'}</span>
                    <span className="flex-1 text-white truncate">{issue.title}</span>
                    <span className="text-xs text-gray-500">{issue.storyPoints ?? '-'} pts</span>
                    <Badge variant="outline" className="text-[10px]">{issue.status}</Badge>
                    <Button
                      size="sm"
                      disabled={addingIssueId === issue.id}
                      onClick={async () => {
                        setAddingIssueId(issue.id)
                        try {
                          await api.post(`/projects/${project!.id}/sprints/${activeSprint.id}/issues/${issue.id}`)
                          toast({ title: 'Issue added to sprint' })
                          setSprintIssues((prev) => [...prev, issue])
                          setAvailableIssues((prev) => prev.filter((i) => i.id !== issue.id))
                        } catch {
                          toast({ title: 'Failed to add issue', variant: 'destructive' })
                        } finally { setAddingIssueId(null) }
                      }}
                    >
                      <Link2 className="h-3.5 w-3.5" />
                    </Button>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      )}
      {selectedIssueId && activeSprint && (
        <IssueDetailPanel
          issueId={selectedIssueId}
          projectId={project.id}
          onClose={() => setSelectedIssueId(null)}
          onUpdate={(updated) => setSprintIssues((prev) => prev.map((i) => (i.id === updated.id ? updated : i)))}
        />
      )}
    </div>
  )
}
