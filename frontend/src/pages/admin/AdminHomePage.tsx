import { useEffect, useState } from 'react'
import { Paper, Stack, Typography } from '@mui/material'
import * as api from '../../api/api'

export function AdminHomePage() {
  const [data, setData] = useState<Awaited<ReturnType<typeof api.fetchAdminStats>> | null>(null)

  useEffect(() => {
    void (async () => {
      setData(await api.fetchAdminStats())
    })()
  }, [])

  const items = [
    { label: 'Active employees', value: data?.activeEmployees },
    { label: 'Currently clocked in', value: data?.clockedInNow },
    { label: 'Open / missing punches', value: data?.openMissingPunches },
    { label: 'Overtime candidates (14d)', value: data?.overtimeCandidates },
    { label: 'Pending corrections', value: data?.pendingCorrections },
  ]

  return (
    <div>
      <Typography variant="h4" gutterBottom>
        Administration
      </Typography>
      <Typography color="text.secondary" gutterBottom>
        Summary counts for oversight and follow-up.
      </Typography>
      <Stack direction="row" sx={{ flexWrap: 'wrap', gap: 2, mt: 1 }}>
        {items.map((i) => (
          <Paper key={i.label} sx={{ p: 2, flex: '1 1 240px', minWidth: 200 }}>
            <Typography variant="subtitle2" color="text.secondary">
              {i.label}
            </Typography>
            <Typography variant="h4">{i.value ?? '—'}</Typography>
          </Paper>
        ))}
      </Stack>
    </div>
  )
}
