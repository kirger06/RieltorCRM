import { useEffect, useState } from 'react'
import { getManagerStats, getMyCompany } from '../../api/manager'
import { StatusBadge } from '../../components/StatusBadge'

export default function CompanyStats() {
  const [company, setCompany] = useState<{ name: string } | null>(null)
  const [stats, setStats] = useState<{
    propertyStats: { status: string; count: number }[]
    bookingStats: { type: string; count: number }[]
    totalRevenue: number
    monthlyRevenue?: { period: string; revenue: number; count: number }[]
  } | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([
      getMyCompany().then(r => setCompany(r.data)),
      getManagerStats().then(r => setStats(r.data)),
    ]).finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="loading">Загрузка...</div>

  return (
    <div className="page-container">
      <h1 className="section-title">Статистика — {company?.name}</h1>

      <div className="stats-grid mb-24">
        <div className="stat-card">
          <div className="stat-value">{stats?.totalRevenue?.toLocaleString('ru-RU') ?? 0}</div>
          <div className="stat-label">₽ выручка от продаж</div>
        </div>
        {stats?.bookingStats.map(s => (
          <div key={s.type} className="stat-card">
            <div className="stat-value">{s.count}</div>
            <div className="stat-label"><StatusBadge value={s.type} /></div>
          </div>
        ))}
      </div>

      <h3 className="mb-16">Объекты по статусам</h3>
      <div className="stats-grid mb-24">
        {stats?.propertyStats.map(s => (
          <div key={s.status} className="stat-card">
            <div className="stat-value">{s.count}</div>
            <div className="stat-label"><StatusBadge value={s.status} /></div>
          </div>
        ))}
      </div>
    </div>
  )
}
