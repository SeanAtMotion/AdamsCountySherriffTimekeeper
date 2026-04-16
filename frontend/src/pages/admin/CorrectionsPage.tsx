import { useEffect, useState } from 'react'
import { Box, Button, Paper, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography } from '@mui/material'
import * as api from '../../api/api'
import type { CorrectionRequestDto } from '../../types/api'
import { formatUtcInOffice } from '../../utils/time'

export function CorrectionsPage() {
  const [rows, setRows] = useState<CorrectionRequestDto[]>([])
  const [notes, setNotes] = useState<Record<number, string>>({})

  async function load() {
    setRows(await api.fetchAdminCorrections('Pending'))
  }

  useEffect(() => {
    void load()
  }, [])

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Correction requests
      </Typography>
      <Table size="small" component={Paper}>
        <TableHead>
          <TableRow>
            <TableCell>ID</TableCell>
            <TableCell>Employee</TableCell>
            <TableCell>Entry</TableCell>
            <TableCell>Reason</TableCell>
            <TableCell>Original in</TableCell>
            <TableCell>Review</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((r) => (
            <TableRow key={r.correctionRequestId}>
              <TableCell>{r.correctionRequestId}</TableCell>
              <TableCell>{r.employeeId}</TableCell>
              <TableCell>{r.timeEntryId}</TableCell>
              <TableCell>{r.reason}</TableCell>
              <TableCell>{r.originalEntry ? formatUtcInOffice(r.originalEntry.clockInUtc) : '—'}</TableCell>
              <TableCell>
                <TextField
                  size="small"
                  placeholder="Notes"
                  value={notes[r.correctionRequestId] ?? ''}
                  onChange={(e) => setNotes((m) => ({ ...m, [r.correctionRequestId]: e.target.value }))}
                />
                <Button
                  size="small"
                  onClick={() =>
                    void api.approveCorrection(r.correctionRequestId, notes[r.correctionRequestId]).then(load)
                  }
                >
                  Approve
                </Button>
                <Button
                  size="small"
                  onClick={() => void api.denyCorrection(r.correctionRequestId, notes[r.correctionRequestId]).then(load)}
                >
                  Deny
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  )
}
