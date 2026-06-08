import { useState, useEffect, useCallback } from 'react'
import { useProject } from './useProject'
import api from '@/lib/api'

export function usePermissions() {
  const { project } = useProject()
  const [perms, setPerms] = useState<string[] | null>(null)

  useEffect(() => {
    if (!project) { setPerms(null); return }
    api.get<string[]>(`/projects/${project.id}/my-permissions`).then((r) => setPerms(r.data))
  }, [project])

  const can = useCallback((permission: string) => {
    if (perms === null) return false
    return perms.includes(permission)
  }, [perms])

  return { permissions: perms, can, loading: perms === null }
}
