import { useEffect, useState } from 'react'
import { Alert, Button, Paper, TextField, Typography } from '@mui/material'
import * as api from '../../api/api'
import type { EmployeeProfile } from '../../types/api'

export function ProfilePage() {
  const [profile, setProfile] = useState<EmployeeProfile | null>(null)
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [msg, setMsg] = useState<string | null>(null)
  const [err, setErr] = useState<string | null>(null)

  useEffect(() => {
    void (async () => {
      const p = await api.fetchMyProfile()
      setProfile(p)
      setPhone(p.phone ?? '')
      setEmail(p.email)
    })()
  }, [])

  async function save() {
    setMsg(null)
    setErr(null)
    try {
      await api.updateMyProfile({ phone, email })
      setMsg('Profile updated.')
    } catch {
      setErr('Update failed.')
    }
  }

  if (!profile) return null

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        My profile
      </Typography>
      <Typography>
        {profile.firstName} {profile.lastName} ({profile.employeeNumber})
      </Typography>
      <Typography color="text.secondary" gutterBottom>
        {profile.jobTitle} — {profile.department}
        {profile.division ? ` — ${profile.division}` : ''}
      </Typography>
      {(profile.badgeNumber || profile.supervisorName) && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {profile.badgeNumber ? <>Badge: {profile.badgeNumber}</> : null}
          {profile.badgeNumber && profile.supervisorName ? ' · ' : null}
          {profile.supervisorName ? <>Supervisor: {profile.supervisorName}</> : null}
        </Typography>
      )}
      {msg && <Alert severity="success">{msg}</Alert>}
      {err && <Alert severity="error">{err}</Alert>}
      <TextField label="Email" fullWidth margin="normal" value={email} onChange={(e) => setEmail(e.target.value)} />
      <TextField label="Phone" fullWidth margin="normal" value={phone} onChange={(e) => setPhone(e.target.value)} />
      <Typography variant="caption" sx={{ mt: 2, display: 'block' }}>
        Name, department, division, badge, supervisor, and role are maintained by administration.
      </Typography>
      <Button variant="contained" sx={{ mt: 2 }} onClick={() => void save()}>
        Save changes
      </Button>
    </Paper>
  )
}
