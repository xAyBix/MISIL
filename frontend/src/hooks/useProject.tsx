import { useState, createContext, useContext, useCallback } from 'react'
import type { ProjectDto } from '@/types'

interface ProjectContextType {
  project: ProjectDto | null
  selectProject: (p: ProjectDto) => void
  clearProject: () => void
}

const ProjectContext = createContext<ProjectContextType | null>(null)

export function ProjectProvider({ children }: { children: React.ReactNode }) {
  const [project, setProject] = useState<ProjectDto | null>(null)

  const selectProject = useCallback((p: ProjectDto) => setProject(p), [])
  const clearProject = useCallback(() => setProject(null), [])

  return (
    <ProjectContext.Provider value={{ project, selectProject, clearProject }}>
      {children}
    </ProjectContext.Provider>
  )
}

export function useProject() {
  const ctx = useContext(ProjectContext)
  if (!ctx) throw new Error('useProject must be used within ProjectProvider')
  return ctx
}
