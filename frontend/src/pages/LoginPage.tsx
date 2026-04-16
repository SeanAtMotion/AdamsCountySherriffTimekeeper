import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  FormControlLabel,
  Link,
  TextField,
  Typography,
} from '@mui/material'
import { useAuth } from '../auth/AuthContext'
import { brand } from '../theme'

export function LoginPage() {
  const { login, user } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    if (user) navigate(user.role === 'Admin' ? '/admin' : '/employee', { replace: true })
  }, [user, navigate])

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setBusy(true)
    try {
      await login(username, password, remember)
    } catch {
      setError('Sign-in failed. Check your username and password.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: brand.pageBg,
        p: 2,
      }}
    >
      <Card elevation={2} sx={{ maxWidth: 440, width: 1 }}>
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <Box
              component="img"
              src="/logo.png"
              alt="Adams County, Pennsylvania"
              sx={{ maxWidth: 1, height: 'auto', maxHeight: 80, objectFit: 'contain' }}
            />
            <Typography variant="h5" sx={{ mt: 2, fontFamily: 'Georgia, serif', color: brand.navy }}>
              Adams County Sheriff&apos;s Office
            </Typography>
            <Typography color="text.secondary">Timekeeping</Typography>
          </Box>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <form onSubmit={onSubmit}>
            <TextField
              label="Username"
              fullWidth
              margin="normal"
              autoComplete="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
            <TextField
              label="Password"
              type="password"
              fullWidth
              margin="normal"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
            <FormControlLabel
              control={<Checkbox checked={remember} onChange={(e) => setRemember(e.target.checked)} />}
              label="Remember me"
            />
            <Button type="submit" variant="contained" color="primary" fullWidth sx={{ mt: 2 }} disabled={busy}>
              Sign in
            </Button>
          </form>
          <Typography variant="caption" sx={{ mt: 2, textAlign: 'center', display: 'block' }}>
            Authorized use only.{' '}
            <Link href="#" color="inherit" onClick={(e) => e.preventDefault()}>
              County IT
            </Link>
          </Typography>
        </CardContent>
      </Card>
    </Box>
  )
}
