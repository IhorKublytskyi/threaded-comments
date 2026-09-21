const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5046').replace(/\/+$/, '')

export function apiUrl(path = '') {
  return `${API_BASE_URL}${path}`
}

export { API_BASE_URL }