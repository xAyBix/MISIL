import { useEffect, useState, useCallback } from 'react'
import { useProject } from '@/hooks/useProject'
import { usePermissions } from '@/hooks/usePermissions'
import { useSignalR } from '@/hooks/useSignalR'
import api from '@/lib/api'
import type { IssueDto } from '@/types'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Plus, ListTodo, Trash2, User, Users } from 'lucide-react'
import { useToast } from '@/hooks/use-toast'
import { CreateIssueDialog, type CreateIssueForm } from '@/components/shared/CreateIssueDialog'
import { IssueDetailPanel } from '@/components/shared/IssueDetailPanel'

const typeIcons: Record<string, string> = {
  Task: '🟢', Bug: '🔴', Story: '🟣', Epic: '🟠', Subtask: '⚪',
}

const priorityColors: Record<string, string> = {
  Highest: 'text-red-400', High: 'text-orange-400', Medium: 'text-yellow-400', Low: 'text-gray-400', Lowest: 'text-gray-500',
}

export function BacklogPage() {
  const { project } = useProject()
  const { can } = usePermissions()
  const { toast } = useToast()
  const { joinProject, on } = useSignalR()
  const [issues, setIssues] = useState<IssueDto[]>([])
  const [showCreate, setShowCreate] = useState(false)
  const [selectedIssueId, setSelectedIssueId] = useState<string | null>(null)
  const [memberMap, setMemberMap] = useState<Record<string, string>>({})
  const [teamMap, setTeamMap] = useState<Record<string, string>>({})

  const loadIssues = useCallback(async (pid: string) => {
    const r = await api.get(`/projects/${pid}/issues`)
    setIssues(Array.isArray(r.data) ? r.data : r.data.items ?? [])
  }, [])

  useEffect(() => {
    if (!project) return
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
    return () => { unsub1(); unsub2(); unsub3() }
  }, [project, on, loadIssues])

  if (!project) return null

  const handleDelete = async (issue: IssueDto) => {
    if (!confirm(`Delete "${issue.title}"?`)) return
    try {
      await api.delete(`/projects/${project.id}/issues/${issue.id}`)
      setIssues((prev) => prev.filter((i) => i.id !== issue.id))
      toast({ title: 'Issue deleted' })
    } catch {
      toast({ title: 'Failed to delete issue', variant: 'destructive' })
    }
  }

  const handleCreate = async (data: CreateIssueForm) => {
    const res = await api.post<IssueDto>(`/projects/${project.id}/issues`, {
      title: data.title,
      description: data.description || null,
      type: data.type,
      priority: data.priority,
      storyPoints: data.storyPoints,
      startDate: data.startDate || null,
      dueDate: data.dueDate || null,
      assigneeId: data.assigneeId || null,
      assigneeTeamId: data.assigneeTeamId || null,
      sprintId: data.sprintId || null,
    })
    setIssues((prev) => [...prev, res.data])
  }

  const backlog = issues.filter((i) => i.status !== 'Done')

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-white">Backlog</h1>
        {can('create_issues') && <Button onClick={() => setShowCreate(true)}><Plus className="h-4 w-4" /> Create Issue</Button>}
      </div>
      <div className="bg-surface-1 border border-surface-3 rounded-lg">
        {backlog.length === 0 ? (
          <div className="flex items-center justify-center h-64 text-gray-500 flex-col gap-3">
            <ListTodo className="h-12 w-12" />
            <p className="text-sm">No issues in backlog for {project.name}</p>
          </div>
        ) : (
          <div className="divide-y divide-surface-3">
            {backlog.map((issue) => (
              <div key={issue.id} onClick={() => setSelectedIssueId(issue.id)}
                className="flex items-center gap-3 px-4 py-3 hover:bg-surface-2 transition-colors group cursor-pointer">
                <span className="text-xs">{typeIcons[issue.issueType] ?? '📋'}</span>
                <span className="text-sm text-white flex-1 truncate">{issue.title}</span>
                <span className={`text-[10px] ${priorityColors[issue.priority] ?? ''}`}>{issue.priority}</span>
                <span className="text-[10px] text-gray-500 flex items-center gap-1">
                  {issue.assigneeId ? <><User className="h-3 w-3" />{memberMap[issue.assigneeId] || '...'}</>
                    : issue.assigneeTeamId ? <><Users className="h-3 w-3" />{teamMap[issue.assigneeTeamId] || '...'}</>
                    : null}
                </span>
                <Badge variant="outline" className="text-xs">{issue.status}</Badge>
                {can('create_issues') && (
                  <button
                    onClick={(e) => { e.stopPropagation(); handleDelete(issue) }}
                    className="p-1 rounded text-gray-600 hover:text-red-400 hover:bg-surface-3 opacity-0 group-hover:opacity-100 transition-all"
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </button>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
      <CreateIssueDialog open={showCreate} onClose={() => setShowCreate(false)} onCreate={handleCreate} projectId={project.id} />
      {selectedIssueId && (
        <IssueDetailPanel
          issueId={selectedIssueId}
          projectId={project.id}
          onClose={() => setSelectedIssueId(null)}
          onUpdate={(updated) => setIssues((prev) => prev.map((i) => (i.id === updated.id ? updated : i)))}
        />
      )}
    </div>
  )
}
