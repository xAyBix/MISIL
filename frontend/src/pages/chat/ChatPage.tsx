import { useEffect, useState } from 'react'
import { useProject } from '@/hooks/useProject'
import { usePermissions } from '@/hooks/usePermissions'
import api from '@/lib/api'
import type { ChannelDto, MessageDto, TeamDto } from '@/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Badge } from '@/components/ui/badge'
import { Send, MessageSquare, Plus, X, Users } from 'lucide-react'

export function ChatPage() {
  const { project } = useProject()
  const { can } = usePermissions()
  const [channels, setChannels] = useState<ChannelDto[]>([])
  const [teams, setTeams] = useState<TeamDto[]>([])
  const [messages, setMessages] = useState<MessageDto[]>([])
  const [activeChannel, setActiveChannel] = useState<string | null>(null)
  const [msg, setMsg] = useState('')
  const [showCreateChannel, setShowCreateChannel] = useState(false)
  const [channelName, setChannelName] = useState('')
  const [channelTeamId, setChannelTeamId] = useState('')
  const [creatingChannel, setCreatingChannel] = useState(false)

  useEffect(() => {
    if (!project) return
    api.get<ChannelDto[]>(`/projects/${project.id}/chat/channels`).then((r) => {
      setChannels(r.data)
      if (r.data.length > 0 && !activeChannel) setActiveChannel(r.data[0].id)
    })
    api.get<TeamDto[]>(`/projects/${project.id}/teams`).then((r) => setTeams(r.data))
  }, [project])

  useEffect(() => {
    if (!project || !activeChannel) return
    api.get<MessageDto[]>(`/projects/${project.id}/chat/channels/${activeChannel}/messages`).then((r) => setMessages(r.data))
  }, [project, activeChannel])

  if (!project) return null

  const handleCreateChannel = async () => {
    if (!channelName) return
    setCreatingChannel(true)
    try {
      const res = await api.post<ChannelDto>(`/projects/${project.id}/chat/channels`, {
        name: channelName,
        teamId: channelTeamId || null,
        isPrivate: !!channelTeamId,
      })
      setChannels((prev) => [...prev, res.data])
      setActiveChannel(res.data.id)
      setShowCreateChannel(false)
      setChannelName('')
      setChannelTeamId('')
    } finally { setCreatingChannel(false) }
  }

  const handleSend = async () => {
    if (!msg.trim() || !activeChannel) return
    try {
      const res = await api.post<MessageDto>(`/projects/${project.id}/chat/channels/${activeChannel}/messages`, { content: msg })
      setMessages((prev) => [...prev, res.data])
      setMsg('')
    } catch { /* ignore */ }
  }

  const teamMap = Object.fromEntries(teams.map((t) => [t.id, t.name]))

  return (
    <div className="flex h-full gap-4">
      <div className="w-64 bg-surface-1 rounded-lg border border-surface-3 flex flex-col shrink-0">
        <div className="p-3 border-b border-surface-3 flex items-center justify-between">
          <h2 className="font-semibold text-sm text-white">Channels</h2>
          {can('manage_channels') && <button onClick={() => setShowCreateChannel(true)} className="text-gray-500 hover:text-white"><Plus className="h-4 w-4" /></button>}
        </div>
        <ScrollArea className="flex-1 p-2">
          {channels.length === 0 ? (
            <div className="text-xs text-gray-500 p-2">No channels</div>
          ) : (
            channels.map((ch) => (
              <div
                key={ch.id}
                onClick={() => setActiveChannel(ch.id)}
                className={`flex items-center gap-2 px-2 py-1.5 rounded text-sm cursor-pointer transition-colors ${
                  activeChannel === ch.id ? 'bg-misil-600/20 text-misil-400' : 'text-gray-400 hover:bg-surface-3 hover:text-white'
                }`}
              >
                <MessageSquare className="h-3.5 w-3.5 shrink-0" />
                <span className="flex-1 truncate">{ch.name}</span>
                {ch.teamId && teamMap[ch.teamId] && (
                  <Badge variant="outline" className="text-[9px] px-1 py-0 border-purple-600/30 text-purple-400 bg-purple-600/10 shrink-0">
                    {teamMap[ch.teamId]}
                  </Badge>
                )}
              </div>
            ))
          )}
        </ScrollArea>
      </div>
      <div className="flex-1 bg-surface-1 rounded-lg border border-surface-3 flex flex-col">
        <div className="p-3 border-b border-surface-3">
          <div className="flex items-center gap-2">
            <h2 className="font-semibold text-sm text-white">
              # {channels.find((c) => c.id === activeChannel)?.name ?? 'select a channel'}
            </h2>
            {(() => {
              const ch = channels.find((c) => c.id === activeChannel)
              return ch?.teamId && teamMap[ch.teamId] ? (
                <Badge variant="outline" className="text-[9px] px-1.5 py-0 border-purple-600/30 text-purple-400 bg-purple-600/10">
                  <Users className="h-2.5 w-2.5 mr-1" />{teamMap[ch.teamId]}
                </Badge>
              ) : null
            })()}
          </div>
        </div>
        <ScrollArea className="flex-1 p-4">
          {messages.length === 0 ? (
            <div className="flex items-center justify-center h-full text-gray-500 text-sm">No messages yet</div>
          ) : (
            <div className="space-y-3">
              {messages.map((m) => (
                <div key={m.id} className="flex gap-2">
                  <div className="w-8 h-8 rounded-full bg-misil-700 flex items-center justify-center text-xs text-white shrink-0">
                    {m.userId.slice(0, 2)}
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium text-white">{m.userId.slice(0, 8)}</span>
                      <span className="text-[10px] text-gray-500">{new Date(m.createdAt).toLocaleTimeString()}</span>
                    </div>
                    <p className="text-sm text-gray-300">{m.content}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </ScrollArea>
        <div className="p-3 border-t border-surface-3 flex gap-2">
          <Input
            placeholder="Type a message..."
            value={msg}
            onChange={(e) => setMsg(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSend()}
          />
          <Button size="icon" onClick={handleSend}><Send className="h-4 w-4" /></Button>
        </div>
      </div>
      {showCreateChannel && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={() => setShowCreateChannel(false)}>
          <div className="bg-surface-1 border border-surface-3 rounded-lg p-6 w-full max-w-sm space-y-4" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-white">New Channel</h2>
              <button onClick={() => setShowCreateChannel(false)} className="text-gray-500 hover:text-white"><X className="h-5 w-5" /></button>
            </div>
            <Input placeholder="Channel Name *" value={channelName} onChange={(e) => setChannelName(e.target.value)} />
            <div className="space-y-1">
              <label className="text-xs text-gray-500">Team (optional — leave blank for project-wide)</label>
              <select
                className="w-full bg-surface-2 border border-surface-3 rounded-md px-3 py-2 text-sm text-white outline-none focus:border-misil-500"
                value={channelTeamId}
                onChange={(e) => setChannelTeamId(e.target.value)}
              >
                <option value="">Project-wide channel</option>
                {teams.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
              </select>
            </div>
            <Button className="w-full" onClick={handleCreateChannel} disabled={creatingChannel || !channelName}>
              {creatingChannel ? 'Creating...' : 'Create Channel'}
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
