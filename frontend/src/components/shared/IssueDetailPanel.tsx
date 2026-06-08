import { useEffect, useState } from 'react'
import { X, Send, Paperclip, User, Users } from 'lucide-react'
import api from '@/lib/api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { IssueStatus, IssueType, IssuePriority } from '@/types'
import type { IssueDto, CommentDto, AttachmentDto } from '@/types'
import { useToast } from '@/hooks/use-toast'

interface Props {
  issueId: string
  projectId: string
  onClose: () => void
  onUpdate: (issue: IssueDto) => void
}

const typeColors: Record<string, string> = {
  Epic: 'border-purple-600/30 text-purple-400 bg-purple-600/10',
  Story: 'border-blue-600/30 text-blue-400 bg-blue-600/10',
  Task: 'border-misil-600/30 text-misil-400 bg-misil-600/10',
  Bug: 'border-red-600/30 text-red-400 bg-red-600/10',
  Subtask: 'border-gray-600/30 text-gray-400 bg-gray-600/10',
}

const priorityColors: Record<string, string> = {
  Highest: 'text-red-400', High: 'text-orange-400', Medium: 'text-yellow-400', Low: 'text-gray-400', Lowest: 'text-gray-500',
}

const statusColors: Record<string, string> = {
  ToDo: 'text-gray-400', InProgress: 'text-blue-400', InReview: 'text-amber-400', Done: 'text-misil-400', Cancelled: 'text-red-400',
}

export function IssueDetailPanel({ issueId, projectId, onClose, onUpdate }: Props) {
  const { toast } = useToast()
  const [issue, setIssue] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [comment, setComment] = useState('')
  const [memberMap, setMemberMap] = useState<Record<string, string>>({})
  const [teamMap, setTeamMap] = useState<Record<string, string>>({})

  useEffect(() => {
    api.get(`/projects/${projectId}/members`).then((r) => {
      const map: Record<string, string> = {}
      r.data.forEach((m: any) => { map[m.userId] = m.userName })
      setMemberMap(map)
    })
    api.get<{ id: string; name: string }[]>(`/projects/${projectId}/teams`).then((r) => {
      const map: Record<string, string> = {}
      r.data.forEach((t: any) => { map[t.id] = t.name })
      setTeamMap(map)
    })
  }, [projectId])

  useEffect(() => {
    if (!issueId) return
    setLoading(true)
    api.get(`/projects/${projectId}/issues/${issueId}`).then((r) => {
      setIssue(r.data)
      setTitle(r.data.title)
      setDescription(r.data.description ?? '')
    }).finally(() => setLoading(false))
  }, [issueId, projectId])

  if (!issueId) return null

  const handleSaveTitle = async () => {
    if (!title.trim()) return
    try {
      const res = await api.put<IssueDto>(`/projects/${projectId}/issues/${issueId}`, { title })
      setIssue((prev: any) => ({ ...prev, ...res.data }))
      onUpdate(res.data)
      toast({ title: 'Title updated' })
    } catch { toast({ title: 'Failed to update title', variant: 'destructive' }) }
  }

  const handleSaveDescription = async () => {
    try {
      const res = await api.put<IssueDto>(`/projects/${projectId}/issues/${issueId}`, { description: description || null })
      setIssue((prev: any) => ({ ...prev, ...res.data }))
      onUpdate(res.data)
      toast({ title: 'Description updated' })
    } catch { toast({ title: 'Failed to update description', variant: 'destructive' }) }
  }

  const handleAddComment = async () => {
    if (!comment.trim()) return
    try {
      const res = await api.post(`/projects/${projectId}/issues/${issueId}/comments`, { content: comment })
      setIssue((prev: any) => ({ ...prev, comments: [...prev.comments, res.data] }))
      setComment('')
    } catch { toast({ title: 'Failed to add comment', variant: 'destructive' }) }
  }

  const handleTypeChange = async (type: string) => {
    try {
      const res = await api.put<IssueDto>(`/projects/${projectId}/issues/${issueId}`, { type })
      setIssue((prev: any) => ({ ...prev, ...res.data }))
      onUpdate(res.data)
    } catch { toast({ title: 'Failed to update type', variant: 'destructive' }) }
  }

  const handlePriorityChange = async (priority: string) => {
    try {
      const res = await api.put<IssueDto>(`/projects/${projectId}/issues/${issueId}`, { priority })
      setIssue((prev: any) => ({ ...prev, ...res.data }))
      onUpdate(res.data)
    } catch { toast({ title: 'Failed to update priority', variant: 'destructive' }) }
  }

  if (loading) return null

  return (
    <div className="fixed inset-0 z-50 flex justify-end">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} />
      <div className="relative w-full max-w-lg bg-surface-1 border-l border-surface-3 overflow-y-auto">
        <div className="sticky top-0 bg-surface-1 z-10 flex items-center justify-between p-4 border-b border-surface-3">
          <h2 className="text-lg font-semibold text-white">Issue Detail</h2>
          <button onClick={onClose} className="text-gray-500 hover:text-white"><X className="h-5 w-5" /></button>
        </div>

        <div className="p-4 space-y-4">
          {/* Title */}
          <div className="flex gap-2">
            <Input value={title} onChange={(e) => setTitle(e.target.value)} onBlur={handleSaveTitle} className="text-lg font-semibold text-white" />
          </div>

          {/* Type & Priority */}
          <div className="flex gap-2">
            <select value={issue?.issueType} onChange={(e) => handleTypeChange(e.target.value)}
              className="bg-surface-2 border border-surface-3 rounded px-2 py-1 text-xs text-white outline-none focus:border-misil-500">
              {Object.values(IssueType).map((t) => <option key={t} value={t}>{t}</option>)}
            </select>
            <select value={issue?.priority} onChange={(e) => handlePriorityChange(e.target.value)}
              className="bg-surface-2 border border-surface-3 rounded px-2 py-1 text-xs text-white outline-none focus:border-misil-500">
              {Object.values(IssuePriority).map((p) => <option key={p} value={p}>{p}</option>)}
            </select>
            <Badge variant="outline" className={statusColors[issue?.status ?? '']}>
              {issue?.status}
            </Badge>
          </div>

          {/* Assignee */}
          <div className="text-sm text-gray-400 flex items-center gap-2">
            {issue?.assigneeId ? <><User className="h-3.5 w-3.5" /> {memberMap[issue.assigneeId] || '...'}</>
              : issue?.assigneeTeamId ? <><Users className="h-3.5 w-3.5" /> {teamMap[issue.assigneeTeamId] || '...'}</>
              : <span className="text-gray-500">Unassigned</span>}
          </div>

          {/* Description */}
          <div>
            <label className="text-xs text-gray-500 font-medium mb-1 block">Description</label>
            <textarea value={description} onChange={(e) => setDescription(e.target.value)}
              onBlur={handleSaveDescription}
              className="w-full bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500 resize-none h-28"
              placeholder="Add a description..." />
          </div>

          {/* Comments */}
          <div>
            <label className="text-xs text-gray-500 font-medium mb-2 block">Comments ({issue?.comments?.length ?? 0})</label>
            <div className="space-y-3 mb-3 max-h-48 overflow-y-auto">
              {(issue?.comments ?? []).length === 0 ? (
                <p className="text-sm text-gray-500">No comments yet</p>
              ) : (
                issue.comments.map((c: CommentDto) => (
                  <div key={c.id} className="flex gap-2 text-sm">
                    <div className="w-6 h-6 rounded-full bg-misil-700 flex items-center justify-center text-[10px] text-white shrink-0">
                      {c.userId.slice(0, 2)}
                    </div>
                    <div>
                      <p className="text-white">{c.content}</p>
                      <p className="text-[10px] text-gray-500">{new Date(c.createdAt).toLocaleString()}</p>
                    </div>
                  </div>
                ))
              )}
            </div>
            <div className="flex gap-2">
              <Input placeholder="Write a comment..." value={comment} onChange={(e) => setComment(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleAddComment()} />
              <Button size="icon" onClick={handleAddComment}><Send className="h-4 w-4" /></Button>
            </div>
          </div>

          {/* Attachments */}
          {(issue?.attachments?.length ?? 0) > 0 && (
            <div>
              <label className="text-xs text-gray-500 font-medium mb-2 block">Attachments</label>
              <div className="space-y-1">
                {issue.attachments.map((a: AttachmentDto) => (
                  <div key={a.id} className="flex items-center gap-2 text-sm text-gray-400">
                    <Paperclip className="h-3.5 w-3.5" />
                    <span>{a.fileName}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
