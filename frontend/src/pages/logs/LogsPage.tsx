import { useEffect, useState } from 'react'
import { useProject } from '@/hooks/useProject'
import api from '@/lib/api'
import type { AuditLogEntryDto, PagedResult } from '@/types'
import { EntityType } from '@/types'
import { ScrollText, ChevronLeft, ChevronRight, Filter } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'

const entityColors: Record<string, string> = {
  Project: 'bg-blue-600/20 text-blue-400 border-blue-600/30',
  Issue: 'bg-misil-600/20 text-misil-400 border-misil-600/30',
  Sprint: 'bg-amber-600/20 text-amber-400 border-amber-600/30',
  Team: 'bg-purple-600/20 text-purple-400 border-purple-600/30',
  ChatMessage: 'bg-green-600/20 text-green-400 border-green-600/30',
  Member: 'bg-rose-600/20 text-rose-400 border-rose-600/30',
  Role: 'bg-cyan-600/20 text-cyan-400 border-cyan-600/30',
}

const actionLabels: Record<string, string> = {
  created: 'created',
  updated: 'updated',
  deleted: 'deleted',
  status_changed: 'changed status',
  assigned: 'assigned',
  added_to_sprint: 'added to sprint',
  removed_from_sprint: 'removed from sprint',
  started: 'started',
  completed: 'completed',
  added: 'added member',
  removed: 'removed member',
  role_changed: 'changed role',
}

export function LogsPage() {
  const { project } = useProject()
  const [logs, setLogs] = useState<AuditLogEntryDto[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize] = useState(20)
  const [filterType, setFilterType] = useState<string>('')
  const [filterFrom, setFilterFrom] = useState('')
  const [filterTo, setFilterTo] = useState('')

  const fetchLogs = async () => {
    if (!project) return
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
    if (filterType) params.set('entityType', filterType)
    if (filterFrom) params.set('from', filterFrom)
    if (filterTo) params.set('to', filterTo)
    const res = await api.get<PagedResult<AuditLogEntryDto>>(`/projects/${project.id}/logs?${params}`)
    setLogs(res.data.items)
    setTotal(res.data.total)
  }

  useEffect(() => { fetchLogs() }, [project, page, filterType, filterFrom, filterTo])
  useEffect(() => { setPage(1) }, [filterType, filterFrom, filterTo])

  if (!project) return null

  const totalPages = Math.ceil(total / pageSize)

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <ScrollText className="h-6 w-6 text-misil-500" />
          <div>
            <h1 className="text-2xl font-bold text-white">Activity Log</h1>
            <p className="text-sm text-gray-500">{project.name}</p>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-surface-1 border border-surface-3 rounded-lg p-3 flex items-center gap-3 flex-wrap">
        <Filter className="h-4 w-4 text-gray-500 shrink-0" />
        <select
          className="bg-surface-2 border border-surface-3 rounded px-2 py-1.5 text-xs text-white outline-none focus:border-misil-500"
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
        >
          <option value="">All types</option>
          {Object.values(EntityType).map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <Input type="date" placeholder="From" value={filterFrom} onChange={(e) => setFilterFrom(e.target.value)} className="w-36 text-xs h-8" />
        <Input type="date" placeholder="To" value={filterTo} onChange={(e) => setFilterTo(e.target.value)} className="w-36 text-xs h-8" />
        <span className="text-xs text-gray-500 ml-auto">{total} event{total !== 1 ? 's' : ''}</span>
      </div>

      {/* Log list */}
      <div className="bg-surface-1 border border-surface-3 rounded-lg divide-y divide-surface-3">
        {logs.length === 0 ? (
          <div className="flex items-center justify-center h-64 text-gray-500 flex-col gap-3">
            <ScrollText className="h-12 w-12" />
            <p className="text-sm">No activity found</p>
          </div>
        ) : (
          logs.map((log, i) => {
            const actionLabel = actionLabels[log.action] ?? log.action
            return (
              <div key={log.id} className="px-4 py-3 flex items-start gap-3 hover:bg-white/[0.02] transition-colors">
                {/* Timeline dot */}
                <div className="relative flex flex-col items-center pt-1.5">
                  <div className={`w-2 h-2 rounded-full ${entityColors[log.entityType]?.split(' ')[0] ?? 'bg-gray-600'}`} />
                  {i < logs.length - 1 && <div className="w-px h-full bg-surface-3 mt-1" />}
                </div>
                {/* Content */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap">
                    <span className="text-sm text-white font-medium">{log.action === 'assigned' ? 'Issue assigned' : `${log.entityType} ${actionLabel}`}</span>
                    <Badge variant="outline" className={`text-[10px] ${entityColors[log.entityType] ?? ''}`}>
                      {log.entityType}
                    </Badge>
                  </div>
                  <p className="text-xs text-gray-400 mt-0.5">
                    {log.userName && <span className="text-gray-500">{log.userName}</span>}
                    {log.userName && log.newValue && log.action !== 'assigned' && <span className="text-gray-600"> &middot; </span>}
                    {log.newValue && log.action !== 'assigned' && <span>{log.newValue}</span>}
                  </p>
                  <p className="text-[10px] text-gray-600 mt-0.5">
                    {new Date(log.createdAt).toLocaleDateString('en-US', {
                      month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit',
                    })}
                  </p>
                </div>
              </div>
            )
          })
        )}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2">
          <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="text-sm text-gray-500">
            Page {page} of {totalPages}
          </span>
          <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      )}
    </div>
  )
}
