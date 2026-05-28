const BASE = import.meta.env.VITE_API_URL ?? ''

export const api = {
  getEmployees: () =>
    fetch(`${BASE}/employees`).then(r => r.json()),

  clockIn: (employeeId: string) =>
    fetch(`${BASE}/attendances/clock-in`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ employeeId }),
    }),

  clockOut: (employeeId: string) =>
    fetch(`${BASE}/attendances/clock-out`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ employeeId }),
    }),

  getMonthlySummary: (employeeId: string, year: number, month: number) =>
    fetch(`${BASE}/attendances/${employeeId}/monthly?year=${year}&month=${month}`)
      .then(r => r.json()),

  getHistory: (employeeId: string) =>
    fetch(`${BASE}/attendances/${employeeId}/history`).then(r => r.json()),

  downloadCsv: (employeeId: string, year: number, month: number) =>
    fetch(`${BASE}/attendances/${employeeId}/monthly/csv?year=${year}&month=${month}`),

  demoReset: () =>
    fetch(`${BASE}/demo/reset`, { method: 'POST' }),
}
