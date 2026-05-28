export interface Employee {
  id: string
  name: string
  hourlyWage: number
  roundUnitMinutes: number
}

export interface AttendanceLog {
  id: number
  employeeId: string
  clockIn: string | null
  clockOut: string | null
  isCorrected: boolean
}

export interface MonthlySummary {
  employeeId: string
  year: number
  month: number
  workDays: number
  totalHours: number
  overtimeHours: number
}
