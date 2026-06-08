import { useEffect, useRef, useCallback } from 'react'
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr'
import { useAuth } from './useAuth'

const connectionRef: { current: HubConnection | null } = { current: null }

export function useSignalR() {
  const { user } = useAuth()
  const connRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    if (!user || connRef.current) return
    const token = localStorage.getItem('token')
    if (!token) return

    const conn = new HubConnectionBuilder()
      .withUrl('/hubs/project', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    conn.start().catch(() => {})
    connRef.current = conn
    connectionRef.current = conn

    return () => {
      conn.stop()
      connRef.current = null
      connectionRef.current = null
    }
  }, [user])

  const joinProject = useCallback((projectId: string) => {
    connRef.current?.invoke('JoinProject', projectId).catch(() => {})
  }, [])

  const leaveProject = useCallback((projectId: string) => {
    connRef.current?.invoke('LeaveProject', projectId).catch(() => {})
  }, [])

  const on = useCallback((event: string, handler: (...args: any[]) => void) => {
    connRef.current?.on(event, handler)
    return () => connRef.current?.off(event, handler)
  }, [])

  return { joinProject, leaveProject, on }
}

export function getSignalRConnection() {
  return connectionRef.current
}
