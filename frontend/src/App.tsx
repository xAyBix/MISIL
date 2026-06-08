import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from '@/hooks/useAuth'
import { AppShell } from '@/components/layout/AppShell'
import { Toaster } from '@/components/ui/toaster'
import { LoginPage } from '@/pages/auth/LoginPage'
import { RegisterPage } from '@/pages/auth/RegisterPage'
import { ProjectListPage } from '@/pages/projects/ProjectListPage'
import { BoardPage } from '@/pages/board/BoardPage'
import { BacklogPage } from '@/pages/backlog/BacklogPage'
import { SprintDetailPage } from '@/pages/sprint/SprintDetailPage'
import { ChatPage } from '@/pages/chat/ChatPage'
import { GanttPage } from '@/pages/gantt/GanttPage'
import { TeamsPage } from '@/pages/teams/TeamsPage'
import { LogsPage } from '@/pages/logs/LogsPage'
import { SettingsPage } from '@/pages/settings/SettingsPage'
import { StatsPage } from '@/pages/stats/StatsPage'
import { MembersPage } from '@/pages/members/MembersPage'
import { RolesPage } from '@/pages/roles/RolesPage'
import { useProject } from '@/hooks/useProject'

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="h-screen flex items-center justify-center text-gray-500">Loading...</div>
  if (!user) return <Navigate to="/login" replace />
  return <>{children}</>
}

function PublicRoute({ children }: { children: React.ReactNode }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="h-screen flex items-center justify-center text-gray-500">Loading...</div>
  if (user) return <Navigate to="/projects" replace />
  return <>{children}</>
}

function RequireProject({ children }: { children: React.ReactNode }) {
  const { project } = useProject()
  if (!project) return <Navigate to="/projects" replace />
  return <>{children}</>
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<PublicRoute><LoginPage /></PublicRoute>} />
          <Route path="/register" element={<PublicRoute><RegisterPage /></PublicRoute>} />
          <Route element={<ProtectedRoute><AppShell /></ProtectedRoute>}>
            <Route path="/projects" element={<ProjectListPage />} />
            <Route path="/board" element={<RequireProject><BoardPage /></RequireProject>} />
            <Route path="/backlog" element={<RequireProject><BacklogPage /></RequireProject>} />
            <Route path="/sprint" element={<RequireProject><SprintDetailPage /></RequireProject>} />
            <Route path="/chat" element={<RequireProject><ChatPage /></RequireProject>} />
            <Route path="/gantt" element={<RequireProject><GanttPage /></RequireProject>} />
            <Route path="/teams" element={<RequireProject><TeamsPage /></RequireProject>} />
            <Route path="/stats" element={<RequireProject><StatsPage /></RequireProject>} />
            <Route path="/members" element={<RequireProject><MembersPage /></RequireProject>} />
            <Route path="/roles" element={<RequireProject><RolesPage /></RequireProject>} />
            <Route path="/logs" element={<RequireProject><LogsPage /></RequireProject>} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="*" element={<Navigate to="/projects" replace />} />
          </Route>
        </Routes>
        <Toaster />
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
