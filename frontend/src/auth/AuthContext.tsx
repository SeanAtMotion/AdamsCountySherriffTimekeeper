import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import * as api from '../api/api'
import { setUnauthorizedHandler } from '../api/http'
import type { UserSummary } from '../types/api'

const AuthContext = createContext<{
  user: UserSummary | null
  loading: boolean
  login: (u: string, p: string, remember: boolean) => Promise<void>
  logout: () => Promise<void>
  refreshUser: () => Promise<void>
} | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserSummary | null>(null)
  const [loading, setLoading] = useState(true)

  const refreshUser = useCallback(async () => {
    try {
      const me = await api.fetchMe()
      setUser({
        employeeId: me.employeeId,
        userName: me.userName,
        fullName: me.fullName,
        role: me.role,
      })
    } catch {
      setUser(null)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void refreshUser()
  }, [refreshUser])

  useEffect(() => {
    setUnauthorizedHandler(() => {
      setUser(null)
    })
    return () => setUnauthorizedHandler(null)
  }, [])

  const login = useCallback(async (username: string, password: string, rememberMe: boolean) => {
    const res = await api.login({ username, password, rememberMe })
    setUser(res.user)
    setLoading(false)
  }, [])

  const logout = useCallback(async () => {
    try {
      await api.logout()
    } catch {
      /* ignore */
    }
    setUser(null)
  }, [])

  const value = useMemo(
    () => ({
      user,
      loading,
      login,
      logout,
      refreshUser,
    }),
    [user, loading, login, logout, refreshUser],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
