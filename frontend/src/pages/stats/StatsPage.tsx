import { useEffect, useState } from 'react'
import { useProject } from '@/hooks/useProject'
import api from '@/lib/api'
import { BarChart3, Bug, CheckCircle, Clock, Users, ListTodo, Timer, Shield, TrendingUp, Target } from 'lucide-react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts'

interface ProjectStats {
  totalIssues: number
  totalMembers: number
  totalSprints: number
  totalTeams: number
  byStatus: { status: string; count: number }[]
  byType: { type: string; count: number }[]
  byPriority: { priority: string; count: number }[]
}

const statusColors: Record<string, string> = {
  ToDo: '#6b7280', InProgress: '#3b82f6', InReview: '#f59e0b', Done: '#10b981', Cancelled: '#ef4444',
}

const typeColors: Record<string, string> = {
  Epic: '#f97316', Story: '#a855f7', Task: '#22c55e', Bug: '#ef4444', Subtask: '#6b7280',
}

const priorityColors: Record<string, string> = {
  Highest: '#ef4444', High: '#f97316', Medium: '#eab308', Low: '#22c55e', Lowest: '#6b7280',
}

const customTooltip = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null
  return (
    <div className="bg-surface-2 border border-surface-3 rounded-lg px-3 py-2 text-sm shadow-xl">
      <p className="text-gray-400 text-xs mb-1">{label}</p>
      {payload.map((p: any, i: number) => (
        <p key={i} className="text-white font-medium">{p.value} {p.name}</p>
      ))}
    </div>
  )
}

export function StatsPage() {
  const { project } = useProject()
  const [stats, setStats] = useState<ProjectStats | null>(null)

  useEffect(() => {
    if (!project) return
    api.get<ProjectStats>(`/projects/${project.id}/stats`).then((r) => setStats(r.data))
  }, [project])

  if (!project) return null

  const doneCount = stats?.byStatus.find((s) => s.status === 'Done')?.count ?? 0
  const total = stats?.totalIssues ?? 0
  const completionPct = total > 0 ? Math.round((doneCount / total) * 100) : 0
  const notDone = total - doneCount
  const pieData = [
    { name: 'Done', value: doneCount },
    { name: 'Open', value: notDone },
  ]

  const cards = [
    { label: 'Total Issues', value: stats?.totalIssues ?? 0, icon: ListTodo, color: 'text-blue-400', sub: `${completionPct}% complete` },
    { label: 'Members', value: stats?.totalMembers ?? 0, icon: Users, color: 'text-green-400', sub: null },
    { label: 'Sprints', value: stats?.totalSprints ?? 0, icon: Timer, color: 'text-amber-400', sub: null },
    { label: 'Teams', value: stats?.totalTeams ?? 0, icon: Shield, color: 'text-purple-400', sub: null },
  ]

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <BarChart3 className="h-6 w-6 text-misil-500" />
        <div>
          <h1 className="text-2xl font-bold text-white">Statistics</h1>
          <p className="text-sm text-gray-500">{project.name}</p>
        </div>
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-4 gap-4">
        {cards.map((c) => (
          <div key={c.label} className="bg-surface-1 border border-surface-3 rounded-lg p-4 hover:border-misil-600/30 transition-colors">
            <div className="flex items-center gap-3">
              <c.icon className={`h-8 w-8 ${c.color}`} />
              <div>
                <p className="text-2xl font-bold text-white">{c.value}</p>
                <p className="text-xs text-gray-500">{c.label}</p>
                {c.sub && <p className="text-[10px] text-misil-500">{c.sub}</p>}
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Charts grid */}
      <div className="grid grid-cols-2 gap-4">
        {/* Completion donut */}
        <div className="bg-surface-1 border border-surface-3 rounded-lg p-4">
          <h3 className="text-sm font-semibold text-white mb-3 flex items-center gap-2"><Target className="h-4 w-4 text-misil-500" /> Completion</h3>
          {total > 0 ? (
            <div className="flex items-center gap-6">
              <ResponsiveContainer width={160} height={160}>
                <PieChart>
                  <Pie data={pieData} cx="50%" cy="50%" innerRadius={50} outerRadius={70} dataKey="value" startAngle={90} endAngle={-270}>
                    <Cell fill="#10b981" />
                    <Cell fill="rgba(255,255,255,0.08)" />
                  </Pie>
                </PieChart>
              </ResponsiveContainer>
              <div className="space-y-2">
                <div className="flex items-center gap-2">
                  <span className="w-2.5 h-2.5 rounded-full bg-misil-500" />
                  <span className="text-sm text-gray-400">Done</span>
                  <span className="text-sm text-white font-medium ml-auto">{doneCount}</span>
                </div>
                <div className="flex items-center gap-2">
                  <span className="w-2.5 h-2.5 rounded-full bg-white/10" />
                  <span className="text-sm text-gray-400">Open</span>
                  <span className="text-sm text-white font-medium ml-auto">{notDone}</span>
                </div>
                <div className="pt-2 border-t border-surface-3">
                  <span className="text-2xl font-bold text-white">{completionPct}%</span>
                  <span className="text-xs text-gray-500 ml-1">complete</span>
                </div>
              </div>
            </div>
          ) : (
            <div className="flex items-center justify-center h-[160px] text-gray-600 text-sm">No data</div>
          )}
        </div>

        {/* By Status - Horizontal bar */}
        <div className="bg-surface-1 border border-surface-3 rounded-lg p-4">
          <h3 className="text-sm font-semibold text-white mb-3 flex items-center gap-2"><CheckCircle className="h-4 w-4 text-misil-500" /> By Status</h3>
          {stats?.byStatus && stats.byStatus.length > 0 ? (
            <ResponsiveContainer width="100%" height={200}>
              <BarChart data={stats.byStatus.map((s) => ({ name: s.status, count: s.count }))} layout="vertical" margin={{ left: 0, right: 0, top: 0, bottom: 0 }}>
                <XAxis type="number" hide />
                <YAxis type="category" dataKey="name" width={80} tick={{ fill: '#9ca3af', fontSize: 12 }} axisLine={false} tickLine={false} />
                <Tooltip content={customTooltip} cursor={{ fill: 'rgba(255,255,255,0.03)' }} />
                <Bar dataKey="count" radius={[0, 4, 4, 0]} barSize={24}>
                  {stats.byStatus.map((s) => (
                    <Cell key={s.status} fill={statusColors[s.status] ?? '#6b7280'} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="flex items-center justify-center h-[200px] text-gray-600 text-sm">No data</div>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        {/* By Type */}
        <div className="bg-surface-1 border border-surface-3 rounded-lg p-4">
          <h3 className="text-sm font-semibold text-white mb-3 flex items-center gap-2"><Bug className="h-4 w-4 text-misil-500" /> By Type</h3>
          {stats?.byType && stats.byType.length > 0 ? (
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={stats.byType.map((t) => ({ name: t.type, count: t.count }))} margin={{ left: 0, right: 0, top: 0, bottom: 0 }}>
                <XAxis dataKey="name" tick={{ fill: '#9ca3af', fontSize: 11 }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fill: '#6b7280', fontSize: 11 }} axisLine={false} tickLine={false} />
                <Tooltip content={customTooltip} cursor={{ fill: 'rgba(255,255,255,0.03)' }} />
                <Bar dataKey="count" radius={[4, 4, 0, 0]} barSize={36}>
                  {stats.byType.map((t) => (
                    <Cell key={t.type} fill={typeColors[t.type] ?? '#6b7280'} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="flex items-center justify-center h-[220px] text-gray-600 text-sm">No data</div>
          )}
        </div>

        {/* By Priority */}
        <div className="bg-surface-1 border border-surface-3 rounded-lg p-4">
          <h3 className="text-sm font-semibold text-white mb-3 flex items-center gap-2"><Clock className="h-4 w-4 text-misil-500" /> By Priority</h3>
          {stats?.byPriority && stats.byPriority.length > 0 ? (
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={stats.byPriority.map((p) => ({ name: p.priority, count: p.count }))} margin={{ left: 0, right: 0, top: 0, bottom: 0 }}>
                <XAxis dataKey="name" tick={{ fill: '#9ca3af', fontSize: 11 }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fill: '#6b7280', fontSize: 11 }} axisLine={false} tickLine={false} />
                <Tooltip content={customTooltip} cursor={{ fill: 'rgba(255,255,255,0.03)' }} />
                <Bar dataKey="count" radius={[4, 4, 0, 0]} barSize={36}>
                  {stats.byPriority.map((p) => (
                    <Cell key={p.priority} fill={priorityColors[p.priority] ?? '#6b7280'} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <div className="flex items-center justify-center h-[220px] text-gray-600 text-sm">No data</div>
          )}
        </div>
      </div>
    </div>
  )
}
