import axios from 'axios'

const baseURL = import.meta.env.VITE_API_BASE ?? '/api'

export const http = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,
})

let onUnauthorized: (() => void) | null = null
export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler
}

function isExpectedUnauthenticatedAuthCall(err: unknown): boolean {
  const status = (err as { response?: { status?: number } })?.response?.status
  if (status !== 401) return false
  const url = String((err as { config?: { url?: string } })?.config?.url ?? '')
  // Not logged in → /auth/me returns 401; wrong password → /auth/login returns 401.
  // Those must not trigger a global redirect/reload (would loop on the login page).
  return url.includes('auth/me') || url.includes('auth/login')
}

http.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err?.response?.status === 401 && onUnauthorized && !isExpectedUnauthenticatedAuthCall(err)) {
      onUnauthorized()
    }
    return Promise.reject(err)
  },
)
