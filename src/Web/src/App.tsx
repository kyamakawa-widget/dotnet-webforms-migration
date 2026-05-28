import { useState, useEffect } from 'react'
import { ClockPanel } from './components/ClockPanel'
import { MonthlySummary } from './components/MonthlySummary'
import { AttendanceHistory } from './components/AttendanceHistory'
import { api } from './api'
import type { Employee } from './types'

type Tab = 'clock' | 'summary' | 'history'

const TABS: { key: Tab; label: string }[] = [
  { key: 'clock',   label: '打刻' },
  { key: 'summary', label: '月次サマリー' },
  { key: 'history', label: '打刻履歴' },
]

export default function App() {
  const [tab, setTab] = useState<Tab>('clock')
  const [employees, setEmployees] = useState<Employee[]>([])
  const [selectedId, setSelectedId] = useState('')

  useEffect(() => {
    api.getEmployees().then((data: Employee[]) => {
      setEmployees(data)
      if (data.length > 0) setSelectedId(data[0].id)
    })
  }, [])

  const selectedEmployee = employees.find(e => e.id === selectedId)

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="bg-slate-800 text-white px-6 py-3 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <span className="text-xl">🕐</span>
          <span className="font-semibold text-lg">勤怠管理システム</span>
        </div>
        <span className="text-emerald-400 text-sm">● DB: Online</span>
      </header>

      <nav className="bg-white border-b border-slate-200">
        <div className="max-w-4xl mx-auto px-6 flex gap-1 pt-2">
          {TABS.map(t => (
            <button
              key={t.key}
              onClick={() => setTab(t.key)}
              className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
                tab === t.key
                  ? 'border-emerald-600 text-emerald-600'
                  : 'border-transparent text-slate-500 hover:text-slate-700'
              }`}
            >
              {t.label}
            </button>
          ))}
        </div>
      </nav>

      <main className="max-w-4xl mx-auto px-6 py-6 pb-16">
        {tab === 'clock' && (
          <ClockPanel
            employees={employees}
            selectedId={selectedId}
            setSelectedId={setSelectedId}
          />
        )}
        {tab === 'summary' && (
          <MonthlySummary
            employeeId={selectedId}
            employeeName={selectedEmployee?.name ?? ''}
          />
        )}
        {tab === 'history' && (
          <AttendanceHistory
            employeeId={selectedId}
            employeeName={selectedEmployee?.name ?? ''}
          />
        )}
      </main>

      <footer className="fixed bottom-0 left-0 right-0 bg-white border-t border-slate-200 px-6 py-2 text-xs text-slate-400 flex justify-between">
        <span>Target: .NET 8 Web API + React　　Database: PostgreSQL</span>
        <span>© 2026 勤怠管理システム</span>
      </footer>
    </div>
  )
}
