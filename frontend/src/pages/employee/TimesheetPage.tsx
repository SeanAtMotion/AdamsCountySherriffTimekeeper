import { useEffect, useMemo, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import * as api from '../../api/api'
import type { TimeEntryDto } from '../../types/api'
import { formatUtcInOffice, minutesToHours } from '../../utils/time'

export function TimesheetPage() {
  const [from, setFrom] = useState(() => {
    const d = new Date()
    d.setDate(d.getDate() - 14)
    return d.toISOString().slice(0, 10)
  })
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10))
  const [rows, setRows] = useState<TimeEntryDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [selected, setSelected] = useState<TimeEntryDto | null>(null)
  const [reason, setReason] = useState('')

  useEffect(() => {
    void (async () => {
      try {
        const data = await api.fetchMyTimesheet(from, to)
        setRows(data)
        setError(null)
      } catch {
        setError('Failed to load timesheet.')
      }
    })()
  }, [from, to])

  const totals = useMemo(() => {
    const m = rows.reduce((a, r) => a + r.totalMinutesWorked, 0)
    const reg = rows.reduce((a, r) => a + r.regularMinutes, 0)
    const ot = rows.reduce((a, r) => a + r.overtimeMinutes, 0)
    return { m, reg, ot }
  }, [rows])

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        My timesheet
      </Typography>
      {error && <Alert severity="error">{error}</Alert>}
      <Paper sx={{ p: 2, my: 2, display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
        <TextField
          label="From"
          type="date"
          slotProps={{ inputLabel: { shrink: true } }}
          value={from}
          onChange={(e) => setFrom(e.target.value)}
        />
        <TextField label="To" type="date" slotProps={{ inputLabel: { shrink: true } }} value={to} onChange={(e) => setTo(e.target.value)} />
      </Paper>
      <Typography>
        Totals: {minutesToHours(totals.m)} hours worked; regular {minutesToHours(totals.reg)} h; overtime{' '}
        {minutesToHours(totals.ot)} h
      </Typography>
      <Table size="small" sx={{ mt: 2 }} component={Paper}>
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>In</TableCell>
            <TableCell>Out</TableCell>
            <TableCell align="right">Hours</TableCell>
            <TableCell>Status</TableCell>
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((r) => (
            <TableRow key={r.timeEntryId}>
              <TableCell>{r.workDate}</TableCell>
              <TableCell>{formatUtcInOffice(r.clockInUtc)}</TableCell>
              <TableCell>{formatUtcInOffice(r.clockOutUtc)}</TableCell>
              <TableCell align="right">{minutesToHours(r.totalMinutesWorked)}</TableCell>
              <TableCell>{r.entryStatus}</TableCell>
              <TableCell>
                <Button size="small" onClick={() => { setSelected(r); setReason(''); setDialogOpen(true); }}>
                  Request correction
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Correction request</DialogTitle>
        <DialogContent>
          <Typography variant="body2" gutterBottom>
            Submit a reason. An administrator will review requested time changes (future enhancement: full edit form).
          </Typography>
          <TextField
            label="Reason"
            fullWidth
            multiline
            minRows={3}
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            disabled={!selected || !reason.trim()}
            onClick={async () => {
              if (!selected) return
              await api.createCorrection({ timeEntryId: selected.timeEntryId, reason: reason.trim() })
              setDialogOpen(false)
            }}
          >
            Submit
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
