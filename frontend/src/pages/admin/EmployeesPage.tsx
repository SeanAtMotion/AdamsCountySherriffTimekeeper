import { useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
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

export function EmployeesPage() {
  const [search, setSearch] = useState('')
  const [rows, setRows] = useState<Awaited<ReturnType<typeof api.fetchEmployees>>>([])
  const [open, setOpen] = useState(false)
  const [err, setErr] = useState<string | null>(null)

  const [form, setForm] = useState({
    employeeNumber: '',
    firstName: '',
    lastName: '',
    email: '',
    userName: '',
    password: '',
    department: '',
    division: '',
    badgeNumber: '',
    supervisorName: '',
    jobTitle: '',
    hireDate: new Date().toISOString().slice(0, 10),
    role: 'Employee' as 'Employee' | 'Admin',
  })

  async function load() {
    setRows(await api.fetchEmployees(search || undefined))
  }

  useEffect(() => {
    void load()
  }, [])

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Employee management
      </Typography>
      <Paper sx={{ p: 2, mb: 2, display: 'flex', gap: 2 }}>
        <TextField
          label="Search"
          size="small"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <Button variant="outlined" onClick={() => void load()}>
          Search
        </Button>
        <Button variant="contained" onClick={() => { setErr(null); setOpen(true); }}>
          New employee
        </Button>
      </Paper>
      {err && <Alert severity="error">{err}</Alert>}
      <Table size="small" component={Paper}>
        <TableHead>
          <TableRow>
            <TableCell>#</TableCell>
            <TableCell>Name</TableCell>
            <TableCell>Department</TableCell>
            <TableCell>Division</TableCell>
            <TableCell>Badge</TableCell>
            <TableCell>Supervisor</TableCell>
            <TableCell>Role</TableCell>
            <TableCell>Active</TableCell>
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((r) => (
            <TableRow key={r.employeeId}>
              <TableCell>{r.employeeNumber}</TableCell>
              <TableCell>
                {r.firstName} {r.lastName}
              </TableCell>
              <TableCell>{r.department}</TableCell>
              <TableCell>{r.division ?? '—'}</TableCell>
              <TableCell>{r.badgeNumber ?? '—'}</TableCell>
              <TableCell>{r.supervisorName ?? '—'}</TableCell>
              <TableCell>{r.role}</TableCell>
              <TableCell>{r.isActive ? 'Yes' : 'No'}</TableCell>
              <TableCell>
                <Button
                  size="small"
                  onClick={() =>
                    void api.patchEmployeeStatus(r.employeeId, !r.isActive).then(load)
                  }
                >
                  {r.isActive ? 'Deactivate' : 'Activate'}
                </Button>
                <Button
                  size="small"
                  onClick={() =>
                    void api.assignRole(r.employeeId, r.role === 'Admin' ? 'Employee' : 'Admin').then(load)
                  }
                >
                  Toggle role
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>New employee</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 1, pt: 1 }}>
          <TextField label="Employee #" value={form.employeeNumber} onChange={(e) => setForm({ ...form, employeeNumber: e.target.value })} />
          <TextField label="First name" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
          <TextField label="Last name" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
          <TextField label="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          <TextField label="Username" value={form.userName} onChange={(e) => setForm({ ...form, userName: e.target.value })} />
          <TextField
            label="Password"
            type="password"
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
          />
          <TextField label="Department" value={form.department} onChange={(e) => setForm({ ...form, department: e.target.value })} />
          <TextField
            label="Division (optional)"
            value={form.division}
            onChange={(e) => setForm({ ...form, division: e.target.value })}
          />
          <TextField
            label="Badge # (optional)"
            value={form.badgeNumber}
            onChange={(e) => setForm({ ...form, badgeNumber: e.target.value })}
          />
          <TextField
            label="Supervisor name (optional)"
            value={form.supervisorName}
            onChange={(e) => setForm({ ...form, supervisorName: e.target.value })}
          />
          <TextField label="Job title" value={form.jobTitle} onChange={(e) => setForm({ ...form, jobTitle: e.target.value })} />
          <TextField
            label="Hire date"
            type="date"
            slotProps={{ inputLabel: { shrink: true } }}
            value={form.hireDate}
            onChange={(e) => setForm({ ...form, hireDate: e.target.value })}
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={form.role === 'Admin'}
                onChange={(e) => setForm({ ...form, role: e.target.checked ? 'Admin' : 'Employee' })}
              />
            }
            label="Administrator"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={async () => {
              try {
                await api.createEmployee({
                  employeeNumber: form.employeeNumber,
                  firstName: form.firstName,
                  lastName: form.lastName,
                  email: form.email,
                  userName: form.userName,
                  password: form.password,
                  department: form.department,
                  division: form.division || undefined,
                  badgeNumber: form.badgeNumber || undefined,
                  supervisorName: form.supervisorName || undefined,
                  jobTitle: form.jobTitle,
                  hireDate: form.hireDate,
                  role: form.role,
                })
                setOpen(false)
                await load()
              } catch {
                setErr('Create failed.')
              }
            }}
          >
            Create
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
