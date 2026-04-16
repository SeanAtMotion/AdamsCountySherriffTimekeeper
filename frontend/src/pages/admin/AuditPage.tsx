import { useEffect, useState } from 'react'
import { Paper, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material'
import * as api from '../../api/api'

export function AuditPage() {
  const [rows, setRows] = useState<Awaited<ReturnType<typeof api.fetchAuditLogs>>>([])

  useEffect(() => {
    void (async () => {
      setRows(await api.fetchAuditLogs())
    })()
  }, [])

  return (
    <div>
      <Typography variant="h4" gutterBottom>
        Audit log
      </Typography>
      <Typography color="text.secondary" gutterBottom>
        Administrative changes to time data.
      </Typography>
      <Table size="small" component={Paper}>
        <TableHead>
          <TableRow>
            <TableCell>Time (UTC)</TableCell>
            <TableCell>Actor</TableCell>
            <TableCell>Action</TableCell>
            <TableCell>Entity</TableCell>
            <TableCell>Details</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((r) => (
            <TableRow key={r.auditLogId}>
              <TableCell>{r.timestampUtc}</TableCell>
              <TableCell>{r.actorEmployeeId ?? '—'}</TableCell>
              <TableCell>{r.actionType}</TableCell>
              <TableCell>
                {r.entityType} #{r.entityId}
              </TableCell>
              <TableCell sx={{ maxWidth: 360, wordBreak: 'break-word', fontSize: 12 }}>
                {r.newValuesJson ?? r.oldValuesJson ?? ''}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
