import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { ProjectProvider } from '@/hooks/useProject'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import { StatusBar } from './StatusBar'

export function AppShell() {
  const [sidebarOpen, setSidebarOpen] = useState(true)

  return (
    <ProjectProvider>
      <div className="h-screen flex flex-col bg-surface-0">
        <TopBar onToggleSidebar={() => setSidebarOpen((v) => !v)} />
        <div className="flex flex-1 overflow-hidden">
          <Sidebar open={sidebarOpen} />
          <main className="flex-1 overflow-y-auto p-6 transition-all duration-200">
            <Outlet />
          </main>
        </div>
        <StatusBar />
      </div>
    </ProjectProvider>
  )
}
