const OFFICE_TZ = 'America/New_York'

export function formatUtcInOffice(iso: string | null | undefined, opts?: Intl.DateTimeFormatOptions) {
  if (!iso) return '—'
  const d = new Date(iso)
  return d.toLocaleString(undefined, {
    timeZone: OFFICE_TZ,
    ...opts,
  })
}

export function minutesToHours(m: number) {
  return (m / 60).toFixed(2)
}
