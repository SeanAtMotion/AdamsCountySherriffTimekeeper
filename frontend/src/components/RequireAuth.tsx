import { Navigate, Outlet } from 'react-router-dom'
import { Box, CircularProgress } from '@mui/material'
import { useAuth } from '../auth/AuthContext'

export function RequireAuth() {
  const { user, loading } = useAuth()
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
        <CircularProgress />
      </Box>
    )
  }
  if (!user) return <Navigate to="/login" replace />
  return <Outlet />
}

export function RequireAdmin() {
  const { user, loading } = useAuth()
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
        <CircularProgress />
      </Box>
    )
  }
  if (!user) return <Navigate to="/login" replace />
  if (user.role !== 'Admin') return <Navigate to="/employee" replace />
  return <Outlet />
}
