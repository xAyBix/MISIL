import { useEffect, useMemo, useRef, useState } from 'react'
import { useProject } from '@/hooks/useProject'
import api from '@/lib/api'
import type { GanttDataDto, GanttIssueDto, GanttDependencyDto } from '@/types'
import { Badge } from '@/components/ui/badge'
import { GitBranch, Calendar } from 'lucide-react'

const statusColors: Record<string, string> = {
  ToDo: 'from-gray-600 to-gray-500 border-gray-500',
  InProgress: 'from-blue-600 to-blue-500 border-blue-400',
  InReview: 'from-amber-600 to-amber-500 border-amber-400',
  Done: 'from-misil-600 to-misil-500 border-misil-400',
}

const statusBg: Record<string, string> = {
  ToDo: 'bg-gray-600/20', InProgress: 'bg-blue-600/20',
  InReview: 'bg-amber-600/20', Done: 'bg-misil-600/20',
}

const typeIcons: Record<string, string> = {
  Task: '🟢', Bug: '🔴', Story: '🟣', Epic: '🟠', Subtask: '⚪',
}

function dayOffset(d: Date, origin: Date) {
  return Math.round((d.getTime() - origin.getTime()) / 86400000)
}

function formatDate(d: Date) {
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${mm}/${dd}`
}

function isWeekend(d: Date) {
  const day = d.getDay()
  return day === 0 || day === 6
}

export function GanttPage() {
  const { project } = useProject()
  const [data, setData] = useState<GanttDataDto | null>(null)
  const [tooltip, setTooltip] = useState<{ issue: GanttIssueDto; x: number; y: number } | null>(null)
  const scrollRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!project) return
    api.get<GanttDataDto>(`/projects/${project.id}/gantt`).then((r) => setData(r.data))
  }, [project])

  const { issues, dependencies, origin, dayCount, days } = useMemo(() => {
    if (!data || data.issues.length === 0) return { issues: [], dependencies: [], origin: new Date(), dayCount: 0, days: [] as Date[] }
    const sorted = [...data.issues].sort((a, b) => (a.startDate ?? a.dueDate ?? '').localeCompare(b.startDate ?? b.dueDate ?? ''))
    const startDates = sorted.map((i) => i.startDate).filter(Boolean) as string[]
    const endDates = sorted.map((i) => i.dueDate).filter(Boolean) as string[]
    const minDate = startDates.length ? new Date(startDates.reduce((a, b) => a < b ? a : b)) : new Date()
    const maxDate = endDates.length ? new Date(endDates.reduce((a, b) => a > b ? a : b)) : new Date()
    const origin = new Date(minDate)
    origin.setDate(origin.getDate() - 2)
    const end = new Date(maxDate)
    end.setDate(end.getDate() + 3)
    const count = Math.max(Math.ceil((end.getTime() - origin.getTime()) / 86400000), 14)
    const dayArr: Date[] = []
    for (let i = 0; i < count; i++) {
      const d = new Date(origin)
      d.setDate(d.getDate() + i)
      dayArr.push(d)
    }
    return { issues: sorted, dependencies: data.dependencies, origin, dayCount: count, days: dayArr }
  }, [data])

  if (!project) return null

  if (!data || data.issues.length === 0) {
    return (
      <div className="space-y-4">
        <h1 className="text-2xl font-bold text-white">Gantt Chart</h1>
        <div className="bg-surface-1 border border-surface-3 rounded-lg flex items-center justify-center h-96 text-gray-500 flex-col gap-3">
          <GitBranch className="h-16 w-16" />
          <p>No timeline data for {project.name}</p>
          <p className="text-xs text-gray-600">Issues need start and due dates to appear on the Gantt chart</p>
        </div>
      </div>
    )
  }

  const leftWidth = 280
  const dayW = 32
  const rowH = 42
  const todayOffset = dayOffset(new Date(), origin)
  const depMap = new Map<string, GanttDependencyDto[]>()
  dependencies.forEach((d) => {
    const arr = depMap.get(d.dependsOnIssueId) ?? []
    arr.push(d)
    depMap.set(d.dependsOnIssueId, arr)
  })
  const depTargets = new Set(dependencies.map((d) => d.issueId))

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <GitBranch className="h-6 w-6 text-misil-500" />
        <div>
          <h1 className="text-2xl font-bold text-white">Gantt Chart</h1>
          <p className="text-sm text-gray-500">{project.name}</p>
        </div>
      </div>

      <div className="bg-surface-1 border border-surface-3 rounded-lg overflow-hidden" ref={scrollRef}>
        <div className="flex" style={{ minWidth: leftWidth + dayCount * dayW }}>
          {/* Left header */}
          <div className="shrink-0 bg-surface-2 border-r border-surface-3 flex items-end pb-1 px-3" style={{ width: leftWidth, height: 52 }}>
            <span className="text-xs font-medium text-gray-500 uppercase tracking-wider">Issues</span>
          </div>
          {/* Day header */}
          <div className="flex" style={{ height: 52 }}>
            {days.map((d, i) => {
              const isWeek = d.getDay() === 1 || i === 0
              const isWeekendDay = isWeekend(d)
              return (
                <div
                  key={i}
                  className={`text-[10px] text-center flex flex-col justify-end pb-1 border-l border-surface-3 ${isWeekendDay ? 'text-gray-600' : 'text-gray-400'} ${isWeek ? 'font-semibold text-misil-400' : ''}`}
                  style={{ width: dayW }}
                >
                  {isWeekendDay ? '' : (d.getDate() === 1 || i === 0 || d.getDate() === 15 || i % 7 === 0 ? formatDate(d) : '')}
                </div>
              )
            })}
          </div>
        </div>

        {/* Scrollable body */}
        <div className="overflow-auto max-h-[600px]" style={{ minWidth: leftWidth + dayCount * dayW }}>
          <div className="relative" style={{ minWidth: leftWidth + dayCount * dayW }}>
            {/* Weekend bands */}
            {days.map((d, i) => isWeekend(d) && (
              <div key={i} className="absolute top-0 h-full bg-white/[0.02] pointer-events-none" style={{ left: leftWidth + i * dayW, width: dayW }} />
            ))}

            {/* Today line */}
            {todayOffset >= 0 && todayOffset < dayCount && (
              <div className="absolute top-0 h-full w-px bg-red-500/60 z-10 pointer-events-none" style={{ left: leftWidth + todayOffset * dayW }}>
                <div className="absolute -top-0.5 left-1/2 -translate-x-1/2 text-[9px] text-red-500 font-bold whitespace-nowrap">Today</div>
              </div>
            )}

            {/* Dependency lines */}
            <svg className="absolute top-0 left-0 w-full h-full pointer-events-none z-5" style={{ left: 0 }}>
              {issues.map((issue) => {
                const deps = depMap.get(issue.id) ?? []
                return deps.map((dep) => {
                  const target = issues.find((i) => i.id === dep.issueId)
                  if (!target) return null
                  const idx1 = issues.indexOf(issue)
                  const idx2 = issues.indexOf(target)
                  if (idx1 === -1 || idx2 === -1) return null
                  const y1 = 56 + idx1 * rowH + rowH / 2
                  const y2 = 56 + idx2 * rowH + rowH / 2
                  const sOff = issue.startDate ? dayOffset(new Date(issue.startDate), origin) : 0
                  const tOff = target.startDate ? dayOffset(new Date(target.startDate), origin) : 0
                  const x1 = leftWidth + (sOff + 0.5) * dayW
                  const x2 = leftWidth + (tOff + 0.5) * dayW
                  const midX = (x1 + x2) / 2
                  return (
                    <g key={`${dep.id}-${dep.issueId}`}>
                      <path
                        d={`M ${x1} ${y1} C ${midX} ${y1}, ${midX} ${y2}, ${x2} ${y2}`}
                        fill="none"
                        stroke="rgba(255,255,255,0.15)"
                        strokeWidth={1.5}
                        strokeDasharray="4 3"
                      />
                      <polygon
                        points={`${x2},${y2} ${x2 - 4},${y2 - 4} ${x2 - 4},${y2 + 4}`}
                        fill="rgba(255,255,255,0.2)"
                      />
                    </g>
                  )
                })
              })}
            </svg>

            {/* Issue rows */}
            {issues.slice(0, 50).map((issue, idx) => {
              const s = issue.startDate ? new Date(issue.startDate) : null
              const e = issue.dueDate ? new Date(issue.dueDate) : null
              const off = s ? Math.max(dayOffset(s, origin), 0) : 0
              const dur = s && e ? Math.max(Math.ceil((e.getTime() - s.getTime()) / 86400000), 1) : 14
              const clampedDur = Math.min(dur, dayCount - off)
              const hasDep = depTargets.has(issue.id)

              return (
                <div
                  key={issue.id}
                  className="flex border-b border-surface-3 hover:bg-white/[0.02] transition-colors"
                  style={{ height: rowH }}
                >
                  {/* Left cell */}
                  <div
                    className="shrink-0 flex items-center gap-2 px-3 border-r border-surface-3 bg-surface-1 sticky left-0 z-10"
                    style={{ width: leftWidth }}
                  >
                    <span className="text-xs shrink-0">{typeIcons[issue.issueType] ?? '📋'}</span>
                    <span className="text-sm text-gray-300 truncate flex-1">{issue.title}</span>
                    {hasDep && <span className="text-[9px] text-amber-500/60 shrink-0">⤷</span>}
                  </div>
                  {/* Timeline cell */}
                  <div className="relative flex-1" style={{ minWidth: dayCount * dayW }}>
                    <div
                      className={`absolute top-1.5 h-[30px] rounded-md bg-gradient-to-r ${statusColors[issue.status] ?? 'from-surface-4 to-surface-3'} border cursor-pointer transition-all duration-150 hover:scale-y-110 hover:z-20 hover:shadow-lg hover:shadow-black/30`}
                      style={{
                        left: off * dayW + 2,
                        width: Math.max(clampedDur * dayW - 4, 20),
                      }}
                      onMouseEnter={(e) => {
                        const rect = (e.currentTarget.closest('[style*="min-width"]') as HTMLElement)?.getBoundingClientRect()
                        if (rect) {
                          setTooltip({ issue, x: e.clientX - rect.left + 10, y: e.clientY - rect.top - 10 })
                        }
                      }}
                      onMouseLeave={() => setTooltip(null)}
                    >
                      {clampedDur * dayW > 70 && (
                        <span className="text-[10px] text-white/80 truncate block px-2 leading-[30px] font-medium">
                          {issue.issueType}: {issue.title}
                        </span>
                      )}
                    </div>
                    {/* Milestone marker */}
                    {issue.dueDate && !issue.startDate && (
                      <div
                        className="absolute top-1 text-lg"
                        style={{ left: off * dayW }}
                      >◆</div>
                    )}
                    {/* Progress indicator dots */}
                    {[25, 50, 75].filter((pct) => clampedDur * dayW > 100).map((pct) => (
                      <div
                        key={pct}
                        className="absolute top-0 h-full w-px bg-white/[0.04] pointer-events-none"
                        style={{ left: (off + clampedDur * pct / 100) * dayW }}
                      />
                    ))}
                  </div>
                </div>
              )
            })}
          </div>

          {/* Tooltip */}
          {tooltip && (
            <div
              className="fixed z-50 bg-surface-2 border border-surface-3 rounded-lg p-3 shadow-xl pointer-events-none"
              style={{ left: tooltip.x, top: tooltip.y }}
            >
              <div className="flex items-center gap-2 mb-1">
                <span className="text-xs">{typeIcons[tooltip.issue.issueType] ?? '📋'}</span>
                <span className="text-sm font-medium text-white">{tooltip.issue.title}</span>
              </div>
              <div className="flex gap-2 text-xs text-gray-400">
                <Badge variant="outline" className="text-[10px]">{tooltip.issue.status}</Badge>
                <Badge variant="outline" className="text-[10px]">{tooltip.issue.issueType}</Badge>
              </div>
              {tooltip.issue.startDate && (
                <p className="text-xs text-gray-500 mt-1">{new Date(tooltip.issue.startDate).toLocaleDateString()} → {tooltip.issue.dueDate ? new Date(tooltip.issue.dueDate).toLocaleDateString() : '...'}</p>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Legend */}
      <div className="flex items-center gap-4 text-xs text-gray-500 bg-surface-1 border border-surface-3 rounded-lg px-4 py-2">
        <span className="text-gray-400 font-medium">Legend:</span>
        {Object.entries(statusColors).map(([status, gradient]) => (
          <span key={status} className="flex items-center gap-1.5">
            <span className={`w-3 h-3 rounded-sm bg-gradient-to-br ${gradient}`} />
            {status}
          </span>
        ))}
        <span className="flex items-center gap-1.5 ml-2">
          <span className="w-4 h-px bg-white/20" style={{ borderTop: '1.5px dashed rgba(255,255,255,0.15)' }} />
          Dependency
        </span>
      </div>
    </div>
  )
}
