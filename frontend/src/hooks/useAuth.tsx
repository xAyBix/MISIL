import { useState, useEffect, useCallback, createContext, useContext } from 'react'
import api from '@/lib/api'
import type { UserDto, AuthResponse, LoginRequest, RegisterRequest } from '@/types'

interface AuthContextType {
  user: UserDto | null
  loading: boolean
  login: (data: LoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => void
  updateUser: (user: UserDto) => void
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (token) {
      api.get<UserDto>('/auth/me')
        .then((res) => setUser(res.data))
        .catch(() => localStorage.removeItem('token'))
        .finally(() => setLoading(false))
    } else {
      setLoading(false)
    }
  }, [])

  const login = useCallback(async (data: LoginRequest) => {
    const res = await api.post<AuthResponse>('/auth/login', data)
    localStorage.setItem('token', res.data.token)
    setUser(res.data.user)
  }, [])

  const register = useCallback(async (data: RegisterRequest) => {
    const res = await api.post<AuthResponse>('/auth/register', data)
    localStorage.setItem('token', res.data.token)
    setUser(res.data.user)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('token')
    setUser(null)
  }, [])

  const updateUser = useCallback((u: UserDto) => setUser(u), [])

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout, updateUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
