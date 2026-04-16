import { useEffect, useState } from 'react'
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
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
import type { TimeEntryDto, TimeEntryStatus } from '../../types/api'
import { formatUtcInOffice, minutesToHours } from '../../utils/time'

export function TimeEntriesAdminPage() {
  const [rows, setRows] = useState<TimeEntryDto[]>([])
  const [dept, setDept] = useState('')
  const [edit, setEdit] = useState<TimeEntryDto | null>(null)
  const [clockIn, setClockIn] = useState('')
  const [clockOut, setClockOut] = useState('')
  const [status, setStatus] = useState<TimeEntryStatus>('Closed')

  async function load() {
    const data = await api.fetchAdminTimeEntries({
      department: dept || undefined,
    })
    setRows(data)
  }

  useEffect(() => {
    void load()
  }, [])

  function openEdit(e: TimeEntryDto) {
    setEdit(e)
    setClockIn(toLocalInput(e.clockInUtc))
    setClockOut(e.clockOutUtc ? toLocalInput(e.clockOutUtc) : '')
    setStatus(e.entryStatus)
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Time entries (admin)
      </Typography>
      <Paper sx={{ p: 2, mb: 2, display: 'flex', gap: 2 }}>
        <TextField label="Department filter" size="small" value={dept} onChange={(e) => setDept(e.target.value)} />
        <Button variant="outlined" onClick={() => void load()}>
          Apply
        </Button>
      </Paper>
      <Table size="small" component={Paper}>
        <TableHead>
          <TableRow>
            <TableCell>ID</TableCell>
            <TableCell>Emp</TableCell>
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
              <TableCell>{r.timeEntryId}</TableCell>
              <TableCell>{r.employeeId}</TableCell>
              <TableCell>{r.workDate}</TableCell>
              <TableCell>{formatUtcInOffice(r.clockInUtc)}</TableCell>
              <TableCell>{formatUtcInOffice(r.clockOutUtc)}</TableCell>
              <TableCell align="right">{minutesToHours(r.totalMinutesWorked)}</TableCell>
              <TableCell>{r.entryStatus}</TableCell>
              <TableCell>
                <Button size="small" onClick={() => openEdit(r)}>
                  Edit
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog open={!!edit} onClose={() => setEdit(null)} fullWidth maxWidth="sm">
        <DialogTitle>Edit time entry {edit?.timeEntryId}</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
          <TextField
            label="Clock in (local)"
            type="datetime-local"
            slotProps={{ inputLabel: { shrink: true } }}
            value={clockIn}
            onChange={(e) => setClockIn(e.target.value)}
          />
          <TextField
            label="Clock out (local)"
            type="datetime-local"
            slotProps={{ inputLabel: { shrink: true } }}
            value={clockOut}
            onChange={(e) => setClockOut(e.target.value)}
          />
          <TextField select label="Status" value={status} onChange={(e) => setStatus(e.target.value as TimeEntryStatus)}>
            {(['Open', 'Closed', 'NeedsReview', 'Corrected'] as const).map((s) => (
              <MenuItem key={s} value={s}>
                {s}
              </MenuItem>
            ))}
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEdit(null)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={async () => {
              if (!edit) return
              const ci = new Date(clockIn).toISOString()
              const co = clockOut ? new Date(clockOut).toISOString() : null
              await api.updateAdminTimeEntry(edit.timeEntryId, {
                clockInUtc: ci,
                clockOutUtc: co,
                breakStartUtc: edit.breakStartUtc,
                breakEndUtc: edit.breakEndUtc,
                notes: edit.notes,
                entryStatus: status,
              })
              setEdit(null)
              await load()
            }}
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

function toLocalInput(iso: string) {
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}
