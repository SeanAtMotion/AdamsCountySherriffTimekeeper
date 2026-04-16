import { http } from './http'
import type {
  AdminDashboardStats,
  CorrectionRequestDto,
  EmployeeDashboardDto,
  EmployeeProfile,
  LoginResponse,
  TimeEntryDto,
  UserSummary,
} from '../types/api'

export async function login(body: {
  username: string
  password: string
  rememberMe: boolean
}) {
  const { data } = await http.post<LoginResponse>('/auth/login', body)
  return data
}

export async function logout() {
  await http.post('/auth/logout')
}

export async function fetchMe(): Promise<UserSummary & { email: string }> {
  const { data } = await http.get<{
    employeeId: number
    userName: string
    fullName: string
    role: 'Admin' | 'Employee'
    email: string
  }>('/auth/me')
  return data
}

export async function fetchMyProfile() {
  const { data } = await http.get<EmployeeProfile>('/employees/me')
  return data
}

export async function updateMyProfile(body: { phone?: string; email?: string }) {
  await http.put('/employees/me', body)
}

export async function fetchEmployeeDashboard() {
  const { data } = await http.get<EmployeeDashboardDto>('/timeentries/dashboard')
  return data
}

export async function clockIn() {
  const { data } = await http.post<TimeEntryDto>('/timeentries/clock-in')
  return data
}

export async function clockOut() {
  const { data } = await http.post<TimeEntryDto>('/timeentries/clock-out')
  return data
}

export async function breakStart() {
  const { data } = await http.post<TimeEntryDto>('/timeentries/break-start')
  return data
}

export async function breakEnd() {
  const { data } = await http.post<TimeEntryDto>('/timeentries/break-end')
  return data
}

export async function fetchMyTimesheet(from?: string, to?: string) {
  const { data } = await http.get<TimeEntryDto[]>('/timeentries/my', {
    params: { from, to },
  })
  return data
}

export async function createCorrection(body: {
  timeEntryId: number
  requestedClockInUtc?: string | null
  requestedClockOutUtc?: string | null
  requestedBreakStartUtc?: string | null
  requestedBreakEndUtc?: string | null
  reason: string
}) {
  await http.post('/corrections', body)
}

export async function fetchMyCorrections() {
  const { data } = await http.get<CorrectionRequestDto[]>('/corrections/my')
  return data
}

// --- Admin ---

export async function fetchAdminStats() {
  const { data } = await http.get<AdminDashboardStats>('/admin/dashboard/stats')
  return data
}

export async function fetchEmployees(search?: string) {
  const { data } = await http.get('/employees', { params: { search } })
  return data as {
    employeeId: number
    employeeNumber: string
    firstName: string
    lastName: string
    email: string
    department: string
    division?: string | null
    badgeNumber?: string | null
    supervisorName?: string | null
    isActive: boolean
    role: string
  }[]
}

export async function createEmployee(body: Record<string, unknown>) {
  await http.post('/employees', body)
}

export async function patchEmployeeStatus(id: number, isActive: boolean) {
  await http.patch(`/employees/${id}/status`, { isActive })
}

export async function assignRole(id: number, role: string) {
  await http.put(`/employees/${id}/role`, { role })
}

export async function fetchAdminTimeEntries(params: Record<string, string | number | boolean | undefined>) {
  const { data } = await http.get<TimeEntryDto[]>('/admin/timeentries', { params })
  return data
}

export async function updateAdminTimeEntry(id: number, body: Record<string, unknown>) {
  await http.put(`/admin/timeentries/${id}`, body)
}

export async function fetchAdminCorrections(status?: string) {
  const { data } = await http.get<CorrectionRequestDto[]>('/admin/corrections', {
    params: { status },
  })
  return data
}

export async function approveCorrection(id: number, reviewNotes?: string) {
  await http.post(`/admin/corrections/${id}/approve`, { reviewNotes })
}

export async function denyCorrection(id: number, reviewNotes?: string) {
  await http.post(`/admin/corrections/${id}/deny`, { reviewNotes })
}

export async function fetchHoursSummary(from: string, to: string) {
  const { data } = await http.get('/admin/reports/hours-summary', { params: { from, to } })
  return data as {
    employeeId: number
    employeeNumber: string
    fullName: string
    department: string
    regularHours: number
    overtimeHours: number
    totalHours: number
  }[]
}

export async function fetchOvertimeReport(from: string, to: string) {
  const { data } = await http.get('/admin/reports/overtime', { params: { from, to } })
  return data as {
    employeeId: number
    fullName: string
    department: string
    overtimeHours: number
  }[]
}

export async function fetchMissingPunches() {
  const { data } = await http.get('/admin/reports/missing-punches')
  return data as {
    timeEntryId: number
    employeeId: number
    fullName: string
    department: string
    workDate: string
    issue: string
  }[]
}

export async function fetchAttendance(from: string, to: string) {
  const { data } = await http.get('/admin/reports/attendance', { params: { from, to } })
  return data as { date: string; headcount: number; totalHours: number }[]
}

export async function downloadHoursCsv(from: string, to: string, filename: string) {
  const { data } = await http.get<Blob>('/admin/reports/export/csv', {
    params: { from, to },
    responseType: 'blob',
  })
  const url = URL.createObjectURL(data)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

export async function fetchAuditLogs() {
  const { data } = await http.get('/admin/auditlogs')
  return data as {
    auditLogId: number
    actorEmployeeId?: number | null
    actionType: string
    entityType: string
    entityId: string
    oldValuesJson?: string | null
    newValuesJson?: string | null
    timestampUtc: string
    ipAddress?: string | null
  }[]
}
