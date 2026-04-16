export type UserRole = 'Admin' | 'Employee'

export interface UserSummary {
  employeeId: number
  userName: string
  fullName: string
  role: UserRole
}

/** Session is the HttpOnly auth cookie issued by the API. */
export interface LoginResponse {
  user: UserSummary
}

export interface EmployeeProfile {
  employeeId: number
  employeeNumber: string
  firstName: string
  lastName: string
  middleInitial?: string | null
  email: string
  phone?: string | null
  department: string
  division?: string | null
  jobTitle: string
  badgeNumber?: string | null
  supervisorName?: string | null
  hireDate: string
  isActive: boolean
  role: UserRole
}

export type ClockSessionState = 'ClockedOut' | 'ClockedIn' | 'OnBreak'

export type TimeEntryStatus = 'Open' | 'Closed' | 'NeedsReview' | 'Corrected'

export interface TimeEntryDto {
  timeEntryId: number
  employeeId: number
  workDate: string
  clockInUtc: string
  clockOutUtc?: string | null
  breakStartUtc?: string | null
  breakEndUtc?: string | null
  notes?: string | null
  entryStatus: TimeEntryStatus
  totalMinutesWorked: number
  totalBreakMinutes: number
  regularMinutes: number
  overtimeMinutes: number
}

export interface EmployeeDashboardDto {
  sessionState: ClockSessionState
  activeEntry: TimeEntryDto | null
  breaksEnabled: boolean
  recentEntries: TimeEntryDto[]
}

export interface AdminDashboardStats {
  activeEmployees: number
  clockedInNow: number
  openMissingPunches: number
  overtimeCandidates: number
  pendingCorrections: number
}

export interface CorrectionRequestDto {
  correctionRequestId: number
  employeeId: number
  timeEntryId: number
  requestedClockInUtc?: string | null
  requestedClockOutUtc?: string | null
  requestedBreakStartUtc?: string | null
  requestedBreakEndUtc?: string | null
  reason: string
  status: 'Pending' | 'Approved' | 'Denied'
  submittedAtUtc: string
  reviewedAtUtc?: string | null
  reviewedByEmployeeId?: number | null
  reviewNotes?: string | null
  originalEntry?: TimeEntryDto
}
