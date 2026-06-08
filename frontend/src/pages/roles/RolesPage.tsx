import { useEffect, useState } from 'react'
import { useProject } from '@/hooks/useProject'
import { usePermissions } from '@/hooks/usePermissions'
import api from '@/lib/api'
import type { Role } from '@/types'
import { Shield, Check, X, Plus, Trash2, Pencil } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useToast } from '@/hooks/use-toast'

const allPermissions = [
  { id: 'view_stats', label: 'View Statistics', desc: 'View project statistics' },
  { id: 'invite_members', label: 'Invite Members', desc: 'Invite new members to the project' },
  { id: 'remove_members', label: 'Remove Members', desc: 'Remove members from the project' },
  { id: 'manage_roles', label: 'Manage Roles', desc: 'Create, edit, and delete roles' },
  { id: 'edit_project', label: 'Edit Project', desc: 'Edit project settings' },
  { id: 'manage_teams', label: 'Manage Teams', desc: 'Create, edit, and delete teams' },
  { id: 'manage_channels', label: 'Manage Channels', desc: 'Create and delete chat channels' },
  { id: 'create_issues', label: 'Create Issues', desc: 'Create new issues' },
  { id: 'manage_sprints', label: 'Manage Sprints', desc: 'Create, start, and complete sprints' },
]

export function RolesPage() {
  const { project } = useProject()
  const { can } = usePermissions()
  const { toast } = useToast()
  const [roles, setRoles] = useState<Role[]>([])
  const [editing, setEditing] = useState<string | null>(null)
  const [editPerms, setEditPerms] = useState<string[]>([])
  const [showCreate, setShowCreate] = useState(false)
  const [createForm, setCreateForm] = useState({ name: '', description: '' })
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    if (!project) return
    api.get<Role[]>(`/projects/${project.id}/roles`).then((r) => setRoles(r.data))
  }, [project])

  if (!project) return null

  const handleToggle = (permId: string) => {
    setEditPerms((prev) => prev.includes(permId) ? prev.filter((p) => p !== permId) : [...prev, permId])
  }

  const handleSave = async (roleId: string) => {
    try {
      await api.patch(`/projects/${project.id}/roles/${roleId}`, { permissions: editPerms.join(',') })
      setRoles((prev) => prev.map((r) => r.id === roleId ? { ...r, permissions: editPerms.join(',') } : r))
      setEditing(null)
      toast({ title: 'Permissions updated' })
    } catch {
      toast({ title: 'Failed to update permissions', variant: 'destructive' })
    }
  }

  const startEdit = (role: Role) => {
    setEditing(role.id)
    setEditPerms(role.permissions ? role.permissions.split(',').filter(Boolean) : [])
  }

  const handleDelete = async (roleId: string) => {
    try {
      await api.delete(`/projects/${project.id}/roles/${roleId}`)
      setRoles((prev) => prev.filter((r) => r.id !== roleId))
      toast({ title: 'Role deleted' })
    } catch {
      toast({ title: 'Failed to delete role', variant: 'destructive' })
    }
  }

  const handleCreate = async () => {
    if (!createForm.name) return
    setBusy(true)
    try {
      const res = await api.post<Role>(`/projects/${project.id}/roles`, {
        name: createForm.name,
        description: createForm.description || null,
        permissions: '',
      })
      setRoles((prev) => [...prev, res.data])
      setShowCreate(false)
      setCreateForm({ name: '', description: '' })
      toast({ title: 'Role created' })
    } catch {
      toast({ title: 'Failed to create role', variant: 'destructive' })
    } finally { setBusy(false) }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Shield className="h-6 w-6 text-misil-500" />
          <div>
            <h1 className="text-2xl font-bold text-white">Roles</h1>
            <p className="text-sm text-gray-500">{project.name}</p>
          </div>
        </div>
        {can('manage_roles') && <Button onClick={() => setShowCreate(true)}><Plus className="h-4 w-4" /> Create Role</Button>}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {roles.length === 0 ? (
          <div className="col-span-2 bg-surface-1 border border-surface-3 rounded-lg p-8 text-center text-gray-500">
            <Shield className="h-12 w-12 mx-auto mb-2 opacity-30" />
            <p>No roles yet</p>
          </div>
        ) : (
          roles.map((role) => {
            const isOwner = role.name === 'Owner'
            const isEditing = editing === role.id
            const perms = role.permissions === 'all'
              ? allPermissions.map((p) => p.id)
              : role.permissions ? role.permissions.split(',').filter(Boolean) : []

            return (
              <div key={role.id} className="bg-surface-1 border border-surface-3 rounded-lg overflow-hidden">
                <div className="flex items-center justify-between p-4 border-b border-surface-3">
                  <div className="flex items-center gap-3">
                    <div className={`w-8 h-8 rounded-lg flex items-center justify-center text-sm font-bold ${isOwner ? 'bg-amber-600/20 text-amber-400' : 'bg-misil-600/20 text-misil-400'}`}>
                      {role.name.charAt(0)}
                    </div>
                    <div>
                      <h3 className="font-semibold text-white">{role.name}</h3>
                      {role.description && <p className="text-xs text-gray-500">{role.description}</p>}
                    </div>
                  </div>
                  <div className="flex items-center gap-1">
                    {!isOwner && can('manage_roles') && (
                      <>
                        {isEditing ? (
                          <button onClick={() => handleSave(role.id)} className="p-1.5 rounded text-misil-500 hover:bg-surface-3 transition-colors">
                            <Check className="h-4 w-4" />
                          </button>
                        ) : (
                          <button onClick={() => startEdit(role)} className="p-1.5 rounded text-gray-500 hover:text-white hover:bg-surface-3 transition-colors">
                            <Pencil className="h-3.5 w-3.5" />
                          </button>
                        )}
                        <button onClick={() => handleDelete(role.id)} className="p-1.5 rounded text-gray-500 hover:text-red-400 hover:bg-surface-3 transition-colors">
                          <Trash2 className="h-3.5 w-3.5" />
                        </button>
                      </>
                    )}
                    {isOwner && (
                      <span className="text-[10px] font-medium text-amber-500/60 uppercase tracking-wider">Locked</span>
                    )}
                  </div>
                </div>
                <div className="p-4 space-y-2">
                  {allPermissions.map((perm) => {
                    const enabled = isEditing ? editPerms.includes(perm.id) : perms.includes(perm.id)
                    return (
                      <div key={perm.id} className="flex items-center gap-3">
                        {isEditing ? (
                          <button
                            onClick={() => handleToggle(perm.id)}
                            className={`w-4 h-4 rounded border flex items-center justify-center transition-colors shrink-0 ${
                              enabled ? 'bg-misil-500 border-misil-500' : 'border-surface-4'
                            }`}
                          >
                            {enabled && <Check className="h-3 w-3 text-white" />}
                          </button>
                        ) : (
                          <div className={`w-4 h-4 rounded border flex items-center justify-center shrink-0 ${
                            enabled ? 'bg-misil-500/30 border-misil-500/50' : 'border-surface-4'
                          }`}>
                            {enabled && <Check className="h-3 w-3 text-misil-400" />}
                          </div>
                        )}
                        <div>
                          <span className="text-sm text-gray-400">{perm.label}</span>
                          <p className="text-[10px] text-gray-600">{perm.desc}</p>
                        </div>
                      </div>
                    )
                  })}
                </div>
              </div>
            )
          })
        )}
      </div>

      {showCreate && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={() => setShowCreate(false)}>
          <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 w-full max-w-sm space-y-4" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-white">Create Role</h2>
              <button onClick={() => setShowCreate(false)} className="text-gray-500 hover:text-white"><X className="h-5 w-5" /></button>
            </div>
            <Input placeholder="Role Name *" value={createForm.name} onChange={(e) => setCreateForm({ ...createForm, name: e.target.value })} />
            <Input placeholder="Description (optional)" value={createForm.description} onChange={(e) => setCreateForm({ ...createForm, description: e.target.value })} />
            <Button className="w-full" onClick={handleCreate} disabled={busy || !createForm.name}>
              {busy ? 'Creating...' : 'Create Role'}
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
