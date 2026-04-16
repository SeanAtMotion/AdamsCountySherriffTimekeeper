import { useEffect, useState } from 'react'
import { Box, Button, Paper, Tab, Table, TableBody, TableCell, TableHead, TableRow, Tabs, TextField, Typography } from '@mui/material'
import * as api from '../../api/api'

export function ReportsPage() {
  const [tab, setTab] = useState(0)
  const [from, setFrom] = useState(() => {
    const d = new Date()
    d.setDate(d.getDate() - 30)
    return d.toISOString().slice(0, 10)
  })
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10))

  const [hours, setHours] = useState<Awaited<ReturnType<typeof api.fetchHoursSummary>>>([])
  const [ot, setOt] = useState<Awaited<ReturnType<typeof api.fetchOvertimeReport>>>([])
  const [miss, setMiss] = useState<Awaited<ReturnType<typeof api.fetchMissingPunches>>>([])
  const [att, setAtt] = useState<Awaited<ReturnType<typeof api.fetchAttendance>>>([])

  useEffect(() => {
    void (async () => {
      setHours(await api.fetchHoursSummary(from, to))
      setOt(await api.fetchOvertimeReport(from, to))
      setMiss(await api.fetchMissingPunches())
      setAtt(await api.fetchAttendance(from, to))
    })()
  }, [from, to])

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Reports
      </Typography>
      <Paper sx={{ p: 2, mb: 2, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
        <TextField label="From" type="date" slotProps={{ inputLabel: { shrink: true } }} value={from} onChange={(e) => setFrom(e.target.value)} />
        <TextField label="To" type="date" slotProps={{ inputLabel: { shrink: true } }} value={to} onChange={(e) => setTo(e.target.value)} />
        <Button variant="outlined" onClick={() => void api.downloadHoursCsv(from, to, `hours-${from}-${to}.csv`)}>
          Export CSV
        </Button>
      </Paper>
      <Tabs value={tab} onChange={(_, v) => setTab(v)}>
        <Tab label="Hours summary" />
        <Tab label="Overtime" />
        <Tab label="Missing / flagged" />
        <Tab label="Attendance" />
      </Tabs>
      {tab === 0 && (
        <Table size="small" sx={{ mt: 2 }} component={Paper}>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell>
              <TableCell>Dept</TableCell>
              <TableCell align="right">Regular</TableCell>
              <TableCell align="right">OT</TableCell>
              <TableCell align="right">Total</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {hours.map((r) => (
              <TableRow key={r.employeeId}>
                <TableCell>{r.fullName}</TableCell>
                <TableCell>{r.department}</TableCell>
                <TableCell align="right">{r.regularHours.toFixed(2)}</TableCell>
                <TableCell align="right">{r.overtimeHours.toFixed(2)}</TableCell>
                <TableCell align="right">{r.totalHours.toFixed(2)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      {tab === 1 && (
        <Table size="small" sx={{ mt: 2 }} component={Paper}>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell>
              <TableCell>Dept</TableCell>
              <TableCell align="right">OT hours</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {ot.map((r) => (
              <TableRow key={r.employeeId}>
                <TableCell>{r.fullName}</TableCell>
                <TableCell>{r.department}</TableCell>
                <TableCell align="right">{r.overtimeHours.toFixed(2)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      {tab === 2 && (
        <Table size="small" sx={{ mt: 2 }} component={Paper}>
          <TableHead>
            <TableRow>
              <TableCell>Employee</TableCell>
              <TableCell>Date</TableCell>
              <TableCell>Issue</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {miss.map((r) => (
              <TableRow key={r.timeEntryId}>
                <TableCell>{r.fullName}</TableCell>
                <TableCell>{r.workDate}</TableCell>
                <TableCell>{r.issue}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      {tab === 3 && (
        <Table size="small" sx={{ mt: 2 }} component={Paper}>
          <TableHead>
            <TableRow>
              <TableCell>Date</TableCell>
              <TableCell align="right">Headcount</TableCell>
              <TableCell align="right">Total hours</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {att.map((r) => (
              <TableRow key={r.date}>
                <TableCell>{r.date}</TableCell>
                <TableCell align="right">{r.headcount}</TableCell>
                <TableCell align="right">{r.totalHours.toFixed(2)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </Box>
  )
}
