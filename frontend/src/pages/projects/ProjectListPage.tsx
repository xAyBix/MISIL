import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '@/lib/api'
import type { ProjectDto, ProjectInvitation } from '@/types'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { useProject } from '@/hooks/useProject'
import { useToast } from '@/hooks/use-toast'
import { Plus, FolderKanban, X, Check, Ban, Users, ArrowRight, Sparkles } from 'lucide-react'

const bannerGradients = [
  'from-misil-900/40 via-misil-800/20 to-transparent',
  'from-blue-900/30 via-blue-800/20 to-transparent',
  'from-amber-900/30 via-amber-800/20 to-transparent',
  'from-purple-900/30 via-purple-800/20 to-transparent',
  'from-emerald-900/30 via-emerald-800/20 to-transparent',
  'from-rose-900/30 via-rose-800/20 to-transparent',
]

export function ProjectListPage() {
  const [projects, setProjects] = useState<ProjectDto[]>([])
  const [invitations, setInvitations] = useState<ProjectInvitation[]>([])
  const [showDialog, setShowDialog] = useState(false)
  const [form, setForm] = useState({ name: '', key: '', description: '' })
  const [busy, setBusy] = useState(false)
  const [actionBusy, setActionBusy] = useState<string | null>(null)
  const navigate = useNavigate()
  const { selectProject } = useProject()
  const { toast } = useToast()

  useEffect(() => {
    api.get<ProjectDto[]>('/projects').then((r) => setProjects(r.data))
    api.get<ProjectInvitation[]>('/invitations/pending').then((r) => setInvitations(r.data))
  }, [])

  const handleAccept = async (inv: ProjectInvitation) => {
    setActionBusy(inv.id)
    try {
      await api.post(`/invitations/${inv.id}/accept`)
      setInvitations((prev) => prev.filter((i) => i.id !== inv.id))
      setProjects((prev) => [...prev, { ...inv.project, memberCount: inv.project.memberCount + 1 }])
      toast({ title: `Joined ${inv.project.name}` })
    } catch {
      toast({ title: 'Failed to accept invitation', variant: 'destructive' })
    } finally { setActionBusy(null) }
  }

  const handleDeny = async (inv: ProjectInvitation) => {
    setActionBusy(inv.id)
    try {
      await api.post(`/invitations/${inv.id}/deny`)
      setInvitations((prev) => prev.filter((i) => i.id !== inv.id))
      toast({ title: 'Invitation declined' })
    } catch {
      toast({ title: 'Failed to decline invitation', variant: 'destructive' })
    } finally { setActionBusy(null) }
  }

  const handleCreate = async () => {
    if (!form.name || !form.key) return
    setBusy(true)
    try {
      const res = await api.post<ProjectDto>('/projects', form)
      setProjects((prev) => [...prev, res.data])
      setShowDialog(false)
      setForm({ name: '', key: '', description: '' })
      toast({ title: 'Project created' })
    } catch {
      toast({ title: 'Failed to create project', variant: 'destructive' })
    } finally { setBusy(false) }
  }

  const handleSelect = (p: ProjectDto) => {
    selectProject(p)
    navigate('/board')
  }

  return (
    <div className="max-w-6xl mx-auto space-y-8">
      {/* Invitations */}
      {invitations.length > 0 && (
        <div className="bg-surface-1 border border-amber-600/30 rounded-xl overflow-hidden">
          <div className="flex items-center gap-2 px-5 py-3 bg-amber-600/5 border-b border-amber-600/20">
            <Sparkles className="h-4 w-4 text-amber-500" />
            <h2 className="text-sm font-semibold text-amber-400">Pending Invitations</h2>
          </div>
          <div className="divide-y divide-surface-3">
            {invitations.map((inv) => (
              <div key={inv.id} className="flex items-center justify-between px-5 py-3 hover:bg-surface-2 transition-colors">
                <div className="flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-amber-600/15 flex items-center justify-center">
                    <FolderKanban className="h-4 w-4 text-amber-500" />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-white">{inv.project.name}</p>
                    <p className="text-[11px] text-gray-500">Invited by {inv.invitedByUser.userName}</p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Button size="sm" onClick={() => handleAccept(inv)} disabled={actionBusy === inv.id}>
                    <Check className="h-3.5 w-3.5" /> Accept
                  </Button>
                  <Button size="sm" variant="outline" onClick={() => handleDeny(inv)} disabled={actionBusy === inv.id}>
                    <Ban className="h-3.5 w-3.5" /> Deny
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-white">Projects</h1>
          <p className="text-sm text-gray-500 mt-1">{projects.length} project{projects.length !== 1 ? 's' : ''}</p>
        </div>
        <Button onClick={() => setShowDialog(true)}>
          <Plus className="h-4 w-4" /> New Project
        </Button>
      </div>

      {/* Project Grid */}
      {projects.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-gray-500 bg-surface-1 border border-surface-3 rounded-xl">
          <FolderKanban className="h-16 w-16 mb-4 text-gray-600" />
          <p className="text-lg font-medium">No projects yet</p>
          <p className="text-sm mt-1">Create your first project to get started</p>
          <Button className="mt-6" onClick={() => setShowDialog(true)}>
            <Plus className="h-4 w-4" /> Create Project
          </Button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
          {projects.map((p, idx) => (
            <div
              key={p.id}
              onClick={() => handleSelect(p)}
              className="group relative bg-surface-1 border border-surface-3 rounded-xl overflow-hidden cursor-pointer hover:border-misil-600/50 hover:shadow-lg hover:shadow-misil-900/20 transition-all duration-200"
            >
              {/* Gradient banner */}
              <div className={`h-20 bg-surface-2 bg-gradient-to-br ${bannerGradients[idx % bannerGradients.length]} relative`}>
                <div className="absolute top-3 left-3">
                  <div className="w-9 h-9 rounded-lg bg-misil-700/80 backdrop-blur-sm flex items-center justify-center shadow-lg">
                    <span className="text-sm font-bold text-white">{p.name.charAt(0).toUpperCase()}</span>
                  </div>
                </div>
                <Badge className="absolute top-3 right-3 bg-black/40 text-white border-none text-[10px]">{p.key}</Badge>
              </div>

              {/* Content */}
              <div className="p-4">
                <div className="flex items-start justify-between mb-2">
                  <h3 className="font-semibold text-white group-hover:text-misil-400 transition-colors">{p.name}</h3>
                  <ArrowRight className="h-4 w-4 text-gray-600 opacity-0 group-hover:opacity-100 group-hover:text-misil-500 transition-all -mt-0.5" />
                </div>
                {p.description && (
                  <p className="text-sm text-gray-500 line-clamp-2 mb-3 leading-relaxed">{p.description}</p>
                )}
                <div className="flex items-center gap-1.5 text-xs text-gray-600">
                  <Users className="h-3.5 w-3.5" />
                  <span>{p.memberCount} member{p.memberCount !== 1 ? 's' : ''}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Create dialog */}
      {showDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={() => setShowDialog(false)}>
          <div className="bg-surface-1 border border-surface-3 rounded-xl p-6 w-full max-w-md space-y-4 shadow-2xl" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-white">New Project</h2>
              <button onClick={() => setShowDialog(false)} className="text-gray-500 hover:text-white">
                <X className="h-5 w-5" />
              </button>
            </div>
            <Input placeholder="Project Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            <Input placeholder="Key (e.g. PROJ)" value={form.key} onChange={(e) => setForm({ ...form, key: e.target.value.toUpperCase() })} maxLength={6} />
            <Input placeholder="Description (optional)" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            <Button className="w-full" onClick={handleCreate} disabled={busy || !form.name || !form.key}>
              {busy ? 'Creating...' : 'Create Project'}
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
