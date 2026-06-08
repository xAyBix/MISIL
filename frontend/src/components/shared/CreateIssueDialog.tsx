import { useEffect, useState } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { toast } from '@/hooks/use-toast'
import { IssueType, IssuePriority } from '@/types'
import api from '@/lib/api'

interface Props {
  open: boolean
  onClose: () => void
  onCreate: (data: CreateIssueForm) => Promise<void>
  projectId: string
}

export interface CreateIssueForm {
  title: string
  description: string
  type: IssueType
  priority: IssuePriority
  storyPoints: number | null
  startDate: string
  dueDate: string
  assigneeId?: string | null
  assigneeTeamId?: string | null
  sprintId?: string | null
}

interface MemberOption {
  userId: string
  userName: string
}

interface TeamOption {
  id: string
  name: string
}

interface SprintOption {
  id: string
  name: string
}

export function CreateIssueDialog({ open, onClose, onCreate, projectId }: Props) {
  const [members, setMembers] = useState<MemberOption[]>([])
  const [teams, setTeams] = useState<TeamOption[]>([])
  const [sprints, setSprints] = useState<SprintOption[]>([])
  const [form, setForm] = useState<CreateIssueForm>({
    title: '',
    description: '',
    type: IssueType.Task,
    priority: IssuePriority.Medium,
    storyPoints: null,
    startDate: '',
    dueDate: '',
    assigneeId: null,
    assigneeTeamId: null,
    sprintId: null,
  })
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    if (!open || !projectId) return
    api.get<MemberOption[]>(`/projects/${projectId}/members`).then((r) => setMembers(r.data))
    api.get<TeamOption[]>(`/projects/${projectId}/teams`).then((r) => setTeams(r.data))
    api.get<SprintOption[]>(`/projects/${projectId}/sprints`).then((r) => setSprints(r.data))
  }, [open, projectId])

  const handleSubmit = async () => {
    if (!form.title) return
    setBusy(true)
    try {
      await onCreate(form)
      setForm({ title: '', description: '', type: IssueType.Task, priority: IssuePriority.Medium, storyPoints: null, startDate: '', dueDate: '', assigneeId: null, assigneeTeamId: null, sprintId: null })
      onClose()
    } catch (e) {
      const msg = (e as any)?.response?.data?.error || (e as Error)?.message || 'Unknown error'
      toast({ title: 'Failed to create issue', description: msg, variant: 'destructive' })
    } finally {
      setBusy(false)
    }
  }

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={onClose}>
      <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 w-full max-w-lg space-y-4" onClick={(e) => e.stopPropagation()}>
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold text-white">Create Issue</h2>
          <button onClick={onClose} className="text-gray-500 hover:text-white"><X className="h-5 w-5" /></button>
        </div>
        <Input placeholder="Title *" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
        <div className="grid grid-cols-2 gap-3">
          <select
            className="bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500"
            value={form.type}
            onChange={(e) => setForm({ ...form, type: e.target.value as IssueType })}
          >
            {Object.values(IssueType).map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <select
            className="bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500"
            value={form.priority}
            onChange={(e) => setForm({ ...form, priority: e.target.value as IssuePriority })}
          >
            {Object.values(IssuePriority).map((p) => <option key={p} value={p}>{p}</option>)}
          </select>
        </div>
        <textarea
          placeholder="Description (optional)"
          className="w-full bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500 resize-none h-20"
          value={form.description}
          onChange={(e) => setForm({ ...form, description: e.target.value })}
        />
        <div className="grid grid-cols-3 gap-3">
          <select
            className="bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500"
            value={form.assigneeId ?? ''}
            onChange={(e) => setForm({ ...form, assigneeId: e.target.value || null, assigneeTeamId: null })}
          >
            <option value="">Member</option>
            {members.map((m) => <option key={m.userId} value={m.userId}>{m.userName}</option>)}
          </select>
          <select
            className="bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500"
            value={form.assigneeTeamId ?? ''}
            onChange={(e) => setForm({ ...form, assigneeTeamId: e.target.value || null, assigneeId: null })}
          >
            <option value="">Team</option>
            {teams.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
          </select>
          <select
            className="bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500"
            value={form.sprintId ?? ''}
            onChange={(e) => setForm({ ...form, sprintId: e.target.value || null })}
          >
            <option value="">No sprint</option>
            {sprints.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
        </div>
        <div className="grid grid-cols-3 gap-3">
          <Input type="number" placeholder="Story pts" value={form.storyPoints ?? ''} onChange={(e) => setForm({ ...form, storyPoints: e.target.value ? Number(e.target.value) : null })} />
          <Input type="date" placeholder="Start date" value={form.startDate} onChange={(e) => setForm({ ...form, startDate: e.target.value })} />
          <Input type="date" placeholder="Due date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} />
        </div>
        <Button className="w-full" onClick={handleSubmit} disabled={busy || !form.title}>
          {busy ? 'Creating...' : 'Create Issue'}
        </Button>
      </div>
    </div>
  )
}
