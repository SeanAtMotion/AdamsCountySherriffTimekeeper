import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  Stack,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import * as api from '../../api/api'
import type { EmployeeDashboardDto } from '../../types/api'
import { formatUtcInOffice, minutesToHours } from '../../utils/time'

function statusLabel(s: EmployeeDashboardDto['sessionState']) {
  switch (s) {
    case 'ClockedIn':
      return <Chip label="Clocked in" color="primary" />
    case 'OnBreak':
      return <Chip label="On break" color="secondary" />
    default:
      return <Chip label="Clocked out" variant="outlined" />
  }
}

export function EmployeeDashboardPage() {
  const [data, setData] = useState<EmployeeDashboardDto | null>(null)
  const [now, setNow] = useState(() => new Date())
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const load = useCallback(async () => {
    try {
      const d = await api.fetchEmployeeDashboard()
      setData(d)
      setError(null)
    } catch {
      setError('Unable to load dashboard.')
    }
  }, [])

  useEffect(() => {
    void load()
    const t = setInterval(() => setNow(new Date()), 1000)
    return () => clearInterval(t)
  }, [load])

  async function act(fn: () => Promise<unknown>) {
    setBusy(true)
    setError(null)
    try {
      await fn()
      await load()
    } catch (e: unknown) {
      const msg = e && typeof e === 'object' && 'response' in e && e.response && typeof e.response === 'object' && 'data' in e.response
        ? String((e.response as { data?: { message?: string } }).data?.message ?? 'Request failed')
        : 'Request failed'
      setError(msg)
    } finally {
      setBusy(false)
    }
  }

  const todayMins =
    data?.recentEntries
      .filter((e) => e.workDate === new Date().toISOString().slice(0, 10))
      .reduce((a, e) => a + e.totalMinutesWorked, 0) ?? 0

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <Typography color="text.secondary" gutterBottom>
        Office times shown in {Intl.DateTimeFormat().resolvedOptions().timeZone} (configure server for official zone).
      </Typography>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      <Paper sx={{ p: 2, mb: 2 }}>
        <Typography variant="subtitle2" color="text.secondary">
          Current time
        </Typography>
        <Typography variant="h5">{now.toLocaleString()}</Typography>
        <Box sx={{ mt: 2, display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
          <Typography variant="subtitle1">Status:</Typography>
          {data ? statusLabel(data.sessionState) : null}
        </Box>
        <Stack direction="row" sx={{ flexWrap: 'wrap', gap: 2, mt: 2 }}>
          <Button variant="contained" size="large" disabled={busy || data?.sessionState !== 'ClockedOut'} onClick={() => act(api.clockIn)}>
            Clock in
          </Button>
          <Button variant="contained" color="secondary" size="large" disabled={busy || data?.sessionState !== 'ClockedIn'} onClick={() => act(api.clockOut)}>
            Clock out
          </Button>
          {data?.breaksEnabled && (
            <>
              <Button variant="outlined" disabled={busy || data.sessionState !== 'ClockedIn'} onClick={() => act(api.breakStart)}>
                Start break
              </Button>
              <Button variant="outlined" disabled={busy || data.sessionState !== 'OnBreak'} onClick={() => act(api.breakEnd)}>
                End break
              </Button>
            </>
          )}
        </Stack>
        <Typography sx={{ mt: 2 }}>Today&apos;s recorded minutes (from closed entries): {todayMins}</Typography>
      </Paper>
      <Typography variant="h6" gutterBottom>
        Recent entries
      </Typography>
      <Table size="small" component={Paper}>
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>In</TableCell>
            <TableCell>Out</TableCell>
            <TableCell align="right">Worked (h)</TableCell>
            <TableCell>Status</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(data?.recentEntries ?? []).map((r) => (
            <TableRow key={r.timeEntryId}>
              <TableCell>{r.workDate}</TableCell>
              <TableCell>{formatUtcInOffice(r.clockInUtc)}</TableCell>
              <TableCell>{formatUtcInOffice(r.clockOutUtc)}</TableCell>
              <TableCell align="right">{minutesToHours(r.totalMinutesWorked)}</TableCell>
              <TableCell>{r.entryStatus}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  )
}
