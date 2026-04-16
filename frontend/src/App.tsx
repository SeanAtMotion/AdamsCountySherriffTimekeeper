import { Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { RequireAdmin, RequireAuth } from './components/RequireAuth'
import { AppLayout } from './layouts/AppLayout'
import { LoginPage } from './pages/LoginPage'
import { EmployeeDashboardPage } from './pages/employee/EmployeeDashboardPage'
import { TimesheetPage } from './pages/employee/TimesheetPage'
import { ProfilePage } from './pages/employee/ProfilePage'
import { AdminHomePage } from './pages/admin/AdminHomePage'
import { EmployeesPage } from './pages/admin/EmployeesPage'
import { TimeEntriesAdminPage } from './pages/admin/TimeEntriesAdminPage'
import { CorrectionsPage } from './pages/admin/CorrectionsPage'
import { ReportsPage } from './pages/admin/ReportsPage'
import { AuditPage } from './pages/admin/AuditPage'

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<RequireAuth />}>
          <Route path="/" element={<Navigate to="/employee" replace />} />
          <Route path="/employee" element={<AppLayout admin={false} />}>
            <Route index element={<EmployeeDashboardPage />} />
            <Route path="timesheet" element={<TimesheetPage />} />
            <Route path="profile" element={<ProfilePage />} />
          </Route>
          <Route element={<RequireAdmin />}>
            <Route path="/admin" element={<AppLayout admin />}>
              <Route index element={<AdminHomePage />} />
              <Route path="employees" element={<EmployeesPage />} />
              <Route path="time-entries" element={<TimeEntriesAdminPage />} />
              <Route path="corrections" element={<CorrectionsPage />} />
              <Route path="reports" element={<ReportsPage />} />
              <Route path="audit" element={<AuditPage />} />
            </Route>
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/employee" replace />} />
      </Routes>
    </AuthProvider>
  )
}
