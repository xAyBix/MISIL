import { useEffect, useState, useCallback, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '@/lib/api'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { useProject } from '@/hooks/useProject'
import { usePermissions } from '@/hooks/usePermissions'
import { useToast } from '@/hooks/use-toast'
import { useSignalR } from '@/hooks/useSignalR'
import { Search, Plus, Trash2, Users, GripVertical } from 'lucide-react'
import type { IssueDto, PagedResult } from '@/types'
import { IssueStatus } from '@/types'
import { CreateIssueDialog, type CreateIssueForm } from '@/components/shared/CreateIssueDialog'
import { IssueDetailPanel } from '@/components/shared/IssueDetailPanel'

const columns = [
  { key: IssueStatus.ToDo, label: 'To Do' },
  { key: IssueStatus.InProgress, label: 'In Progress' },
  { key: IssueStatus.InReview, label: 'In Review' },
  { key: IssueStatus.Done, label: 'Done' },
]

const statusColors: Record<string, string> = {
  ToDo: 'bg-gray-600/20 text-gray-400 border-gray-600/30',
  InProgress: 'bg-blue-600/20 text-blue-400 border-blue-600/30',
  InReview: 'bg-amber-600/20 text-amber-400 border-amber-600/30',
  Done: 'bg-misil-600/20 text-misil-400 border-misil-600/30',
}

const typeIcons: Record<string, string> = {
  Task: '🟢', Bug: '🔴', Story: '🟣', Epic: '🟠', Subtask: '⚪',
}

const priorityColors: Record<string, string> = {
  Highest: 'text-red-400', High: 'text-orange-400', Medium: 'text-yellow-400', Low: 'text-gray-400', Lowest: 'text-gray-500',
}

export function BoardPage() {
  const { project } = useProject()
  const { can } = usePermissions()
  const navigate = useNavigate()
  const { toast } = useToast()
  const { joinProject, on } = useSignalR()
  const dragIssue = useRef<{ id: string; fromStatus: IssueStatus } | null>(null)
  const [issues, setIssues] = useState<IssueDto[]>([])
  const [search, setSearch] = useState('')
  const [showCreate, setShowCreate] = useState(false)
  const [selectedIssueId, setSelectedIssueId] = useState<string | null>(null)
  const [memberMap, setMemberMap] = useState<Record<string, string>>({})
  const [teamMap, setTeamMap] = useState<Record<string, string>>({})

  const loadIssues = useCallback(async (pid: string) => {
    const r = await api.get<PagedResult<IssueDto>>(`/projects/${pid}/issues`)
    setIssues(r.data.items)
  }, [])

  useEffect(() => {
    if (!project) { navigate('/projects'); return }
    loadIssues(project.id)
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
    if (!project) return
    const unsub1 = on('IssueCreated', () => { loadIssues(project.id) })
    const unsub2 = on('IssueUpdated', () => { loadIssues(project.id) })
    const unsub3 = on('IssueDeleted', () => { loadIssues(project.id) })
    const unsub4 = on('IssueStatusChanged', () => { loadIssues(project.id) })
    const unsub5 = on('IssueAssigned', () => { loadIssues(project.id) })
    return () => { unsub1(); unsub2(); unsub3(); unsub4(); unsub5() }
  }, [project, on, loadIssues])

  if (!project) return null

  const handleCreate = async (data: CreateIssueForm) => {
    const res = await api.post<IssueDto>(`/projects/${project.id}/issues`, {
      title: data.title, description: data.description || null, type: data.type,
      priority: data.priority, storyPoints: data.storyPoints,
      startDate: data.startDate || null, dueDate: data.dueDate || null,
      assigneeId: data.assigneeId || null, assigneeTeamId: data.assigneeTeamId || null,
      sprintId: data.sprintId || null,
    })
    setIssues((prev) => [...prev, res.data])
  }

  const filtered = issues.filter((i) => i.title.toLowerCase().includes(search.toLowerCase()))

  const handleDelete = async (issue: IssueDto, e: React.MouseEvent) => {
    e.stopPropagation()
    if (!confirm(`Delete "${issue.title}"?`)) return
    try {
      await api.delete(`/projects/${project.id}/issues/${issue.id}`)
      setIssues((prev) => prev.filter((i) => i.id !== issue.id))
      toast({ title: 'Issue deleted' })
    } catch {
      toast({ title: 'Failed to delete issue', variant: 'destructive' })
    }
  }

  const handleDragStart = (issue: IssueDto) => {
    dragIssue.current = { id: issue.id, fromStatus: issue.status }
  }

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault()
  }

  const handleDrop = async (targetStatus: IssueStatus) => {
    if (!dragIssue.current || dragIssue.current.fromStatus === targetStatus) return
    const issueId = dragIssue.current.id
    const prevStatus = dragIssue.current.fromStatus
    dragIssue.current = null
    setIssues((prev) => prev.map((i) => (i.id === issueId ? { ...i, status: targetStatus } : i)))
    try {
      await api.patch(`/projects/${project.id}/issues/${issueId}/status`, JSON.stringify(targetStatus), {
        headers: { 'Content-Type': 'application/json' },
      })
    } catch {
      setIssues((prev) => prev.map((i) => (i.id === issueId ? { ...i, status: prevStatus } : i)))
      toast({ title: 'Failed to update status', variant: 'destructive' })
    }
  }

  const renderIssueCard = (issue: IssueDto) => (
    <div
      key={issue.id}
      draggable
      onDragStart={() => handleDragStart(issue)}
      onClick={() => handleCardClick(issue)}
      className="bg-surface-2 rounded-md p-3 border border-surface-4 cursor-grab active:cursor-grabbing hover:border-misil-600/50 transition-all group"
    >
      <div className="flex items-center justify-between mb-1">
        <div className="flex items-center gap-1.5">
          <span className="text-xs">{typeIcons[issue.issueType] ?? '📋'}</span>
          <GripVertical className="h-3 w-3 text-gray-600 opacity-0 group-hover:opacity-100 transition-opacity" />
        </div>
        <div className="flex items-center gap-1.5">
          <span className={`text-[10px] font-medium ${priorityColors[issue.priority] ?? ''}`}>{issue.priority}</span>
          <Badge variant="outline" className="text-[10px] px-1.5 py-0">{issue.issueType}</Badge>
        </div>
      </div>
      <p className="text-sm text-white font-medium leading-snug">{issue.title}</p>
      <div className="flex items-center justify-between mt-2">
        <span className="text-[10px] text-gray-500">
          {issue.assigneeTeamId ? (
            <><Users className="h-3 w-3 inline" /> {teamMap[issue.assigneeTeamId] || '...'}</>
          ) : issue.assigneeId ? (
            <>👤 {memberMap[issue.assigneeId] || '...'}</>
          ) : 'unassigned'}
        </span>
        {can('create_issues') && (
          <button
            onClick={(e) => handleDelete(issue, e)}
            className="p-1 rounded text-gray-600 hover:text-red-400 hover:bg-surface-3 opacity-0 group-hover:opacity-100 transition-all"
          >
            <Trash2 className="h-3 w-3" />
          </button>
        )}
      </div>
    </div>
  )

  const handleCardClick = (issue: IssueDto) => {
    setSelectedIssueId(issue.id)
  }

  return (
    <><div className="space-y-4 h-full flex flex-col">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">Board</h1>
          <p className="text-sm text-gray-500">{project.name}</p>
        </div>
        <div className="flex items-center gap-2">
          <div className="relative w-64">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-gray-500" />
            <Input placeholder="Search issues..." className="pl-8" value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
          {can('create_issues') && <Button size="sm" onClick={() => setShowCreate(true)}><Plus className="h-4 w-4" /> Issue</Button>}
        </div>
      </div>
      <div className="flex-1 flex gap-4 overflow-x-auto pb-4">
        {columns.map((col) => {
          const colIssues = filtered.filter((i) => i.status === col.key)
          const teams: Record<string, IssueDto[]> = {}
          const unassigned: IssueDto[] = []
          for (const issue of colIssues) {
            if (issue.assigneeTeamId) {
              if (!teams[issue.assigneeTeamId]) teams[issue.assigneeTeamId] = []
              teams[issue.assigneeTeamId].push(issue)
            } else {
              unassigned.push(issue)
            }
          }
          const teamIds = Object.keys(teams).sort()
          return (
            <div
              key={col.key}
              onDragOver={handleDragOver}
              onDrop={() => handleDrop(col.key)}
              className={`flex-1 min-w-[280px] bg-surface-1 rounded-lg border transition-colors flex flex-col ${dragIssue.current ? 'border-misil-500/50' : 'border-surface-3'}`}
            >
              <div className="p-3 border-b border-surface-3 flex items-center justify-between shrink-0">
                <h3 className="font-medium text-sm text-white">{col.label}</h3>
                <Badge variant="outline" className={statusColors[col.key]}>{colIssues.length}</Badge>
              </div>
              <div className="flex-1 p-2 space-y-2 overflow-y-auto">
                {colIssues.length === 0 ? (
                  <div className="flex items-center justify-center h-24 text-gray-600 text-xs">No issues</div>
                ) : (
                  <div className="space-y-3">
                    {teamIds.map((teamId) => (
                      <div key={teamId}>
                        <div className="flex items-center gap-1.5 px-1 py-1 mb-1 sticky top-0 bg-surface-1 z-10">
                          <Users className="h-3 w-3 text-purple-400 shrink-0" />
                          <span className="text-[10px] font-semibold text-purple-400 uppercase tracking-wider">{teamMap[teamId] || 'Unknown'}</span>
                          <Badge variant="outline" className="text-[10px] px-1 py-0">{teams[teamId].length}</Badge>
                        </div>
                        {teams[teamId].map(renderIssueCard)}
                      </div>
                    ))}
                    {unassigned.length > 0 && (
                      <div>
                        <div className="flex items-center gap-1.5 px-1 py-1 mb-1 sticky top-0 bg-surface-1 z-10">
                          <span className="text-[10px] font-semibold text-gray-500 uppercase tracking-wider">Unassigned</span>
                          <Badge variant="outline" className="text-[10px] px-1 py-0">{unassigned.length}</Badge>
                        </div>
                        {unassigned.map(renderIssueCard)}
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>
          )
        })}
      </div>
    </div>
      <CreateIssueDialog open={showCreate} onClose={() => setShowCreate(false)} onCreate={handleCreate} projectId={project.id} />
      {selectedIssueId && (
        <IssueDetailPanel
          issueId={selectedIssueId}
          projectId={project.id}
          onClose={() => setSelectedIssueId(null)}
          onUpdate={(updated) => setIssues((prev) => prev.map((i) => (i.id === updated.id ? updated : i)))}
        />
      )}</>
  )
}
