import { Link as RouterLink, Outlet, useNavigate } from 'react-router-dom'
import {
  AppBar,
  Box,
  Button,
  Container,
  Divider,
  Drawer,
  List,
  ListItemButton,
  ListItemText,
  Toolbar,
  Typography,
} from '@mui/material'
import { useAuth } from '../auth/AuthContext'
import { brand } from '../theme'

const drawerWidth = 240

export function AppLayout({ admin }: { admin: boolean }) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const employeeLinks = [
    { to: '/employee', label: 'Dashboard' },
    { to: '/employee/timesheet', label: 'My Timesheet' },
    { to: '/employee/profile', label: 'My Profile' },
  ]

  const adminLinks = [
    { to: '/admin', label: 'Admin Home' },
    { to: '/admin/employees', label: 'Employees' },
    { to: '/admin/time-entries', label: 'Time Entries' },
    { to: '/admin/corrections', label: 'Corrections' },
    { to: '/admin/reports', label: 'Reports' },
    { to: '/admin/audit', label: 'Audit Log' },
  ]

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: brand.pageBg }}>
      <AppBar
        position="fixed"
        elevation={0}
        sx={{
          bgcolor: '#fff',
          color: brand.navy,
          borderBottom: `3px solid ${brand.navy}`,
        }}
      >
        <Toolbar sx={{ gap: 2 }}>
          <Box
            component="img"
            src="/logo.png"
            alt="Adams County, Pennsylvania"
            sx={{ height: 48, width: 'auto' }}
          />
          <Box sx={{ flexGrow: 1 }}>
            <Typography variant="h6" component="div" sx={{ fontFamily: 'Georgia, serif', fontWeight: 700 }}>
              Adams County Sheriff&apos;s Office Timekeeping
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {user?.fullName} {user?.role === 'Admin' ? '(Administrator)' : '(Employee)'}
            </Typography>
          </Box>
          {user?.role === 'Admin' && (
            <Button color="inherit" component={RouterLink} to="/employee">
              Employee view
            </Button>
          )}
          {user?.role === 'Admin' && (
            <Button color="inherit" component={RouterLink} to="/admin">
              Admin
            </Button>
          )}
          <Button
            color="inherit"
            onClick={async () => {
              await logout()
              navigate('/login')
            }}
          >
            Sign out
          </Button>
        </Toolbar>
      </AppBar>
      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          [`& .MuiDrawer-paper`]: {
            width: drawerWidth,
            boxSizing: 'border-box',
            mt: '64px',
            bgcolor: '#fff',
            borderRight: `1px solid ${brand.paleBlue}`,
          },
        }}
      >
        <Toolbar />
        <List>
          {(admin ? adminLinks : employeeLinks).map((l) => (
            <ListItemButton key={l.to} component={RouterLink} to={l.to}>
              <ListItemText primary={l.label} />
            </ListItemButton>
          ))}
        </List>
        <Divider sx={{ my: 1 }} />
        {user?.role === 'Admin' && !admin && (
          <List>
            <ListItemButton component={RouterLink} to="/admin">
              <ListItemText primary="Administration" />
            </ListItemButton>
          </List>
        )}
      </Drawer>
      <Box component="main" sx={{ flexGrow: 1, p: 3, width: `calc(100% - ${drawerWidth}px)`, mt: '64px' }}>
        <Container maxWidth="lg">
          <Outlet />
        </Container>
      </Box>
    </Box>
  )
}
