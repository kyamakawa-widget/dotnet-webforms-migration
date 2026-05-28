import { useState, useEffect } from 'react'
import { api } from '../api'
import type { AttendanceLog } from '../types'

interface Props {
  employeeId: string
  employeeName: string
}

const fmt = (dt: string | null) => dt
  ? new Date(dt).toLocaleString('ja-JP', {
      month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit'
    })
  : '-'

const calcHours = (log: AttendanceLog) => {
  if (!log.clockIn || !log.clockOut) return '-'
  const h = (new Date(log.clockOut).getTime() - new Date(log.clockIn).getTime()) / 3_600_000
  return `${h.toFixed(1)}h`
}

export function AttendanceHistory({ employeeId, employeeName }: Props) {
  const [logs, setLogs]       = useState<AttendanceLog[]>([])
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!employeeId) return
    setLoading(true)
    api.getHistory(employeeId).then(setLogs).finally(() => setLoading(false))
  }, [employeeId])

  return (
    <div className="bg-white border border-slate-200 rounded-lg p-8">
      <div className="mb-5">
        <h2 className="text-slate-700 font-semibold">打刻履歴</h2>
        <p className="text-xs text-slate-400 mt-0.5">対象: {employeeId}　{employeeName}</p>
      </div>

      {loading ? (
        <div className="text-slate-400 text-sm">読み込み中...</div>
      ) : (
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-slate-200 text-slate-500 text-left">
              <th className="pb-2 font-medium">出勤</th>
              <th className="pb-2 font-medium">退勤</th>
              <th className="pb-2 font-medium">勤務時間</th>
              <th className="pb-2 font-medium"></th>
            </tr>
          </thead>
          <tbody>
            {logs.length === 0 ? (
              <tr>
                <td colSpan={4} className="py-6 text-center text-slate-400">データなし</td>
              </tr>
            ) : logs.map(log => (
              <tr key={log.id} className="border-b border-slate-100 hover:bg-slate-50">
                <td className="py-2.5 text-slate-700">{fmt(log.clockIn)}</td>
                <td className="py-2.5 text-slate-700">{fmt(log.clockOut)}</td>
                <td className="py-2.5 text-slate-600">{calcHours(log)}</td>
                <td className="py-2.5">
                  {log.isCorrected && (
                    <span className="text-xs bg-amber-100 text-amber-700 px-1.5 py-0.5 rounded">修正済</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
