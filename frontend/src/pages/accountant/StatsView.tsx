import { useEffect, useState } from 'react'
import { getAccountantStats } from '../../api/accountant'
import { StatusBadge } from '../../components/StatusBadge'

interface AgentStat {
  agentName: string
  totalProperties: number
  availableCount: number
  soldCount: number
  pendingCount: number
  revenue: number
}

interface Stats {
  propertyStats: { status: string; count: number }[]
  bookingsByType: { type: string; count: number }[]
  bookingsByStatus: { status: string; count: number }[]
  totalRevenue: number
  reservedRevenue: number
  monthlyRevenue: { period: string; revenue: number; count: number }[]
  agentStats: AgentStat[]
  propertiesByCity: { city: string; count: number }[]
}

export default function StatsView() {
  const [stats, setStats] = useState<Stats | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getAccountantStats()
      .then(r => setStats(r.data))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="loading">Загрузка...</div>
  if (!stats) return null

  return (
    <div className="page-container">
      <h1 className="section-title">Финансовая статистика</h1>

      {/* Key metrics */}
      <div className="stats-grid mb-24">
        <div className="stat-card">
          <div className="stat-value" style={{ fontSize: '1.3rem', color: 'var(--success)' }}>
            {stats.totalRevenue.toLocaleString('ru-RU')} ₽
          </div>
          <div className="stat-label">Выручка от продаж</div>
        </div>
        <div className="stat-card">
          <div className="stat-value" style={{ fontSize: '1.3rem', color: 'var(--primary)' }}>
            {stats.reservedRevenue.toLocaleString('ru-RU')} ₽
          </div>
          <div className="stat-label">Стоимость броней</div>
        </div>
        {stats.bookingsByType.map(s => (
          <div key={s.type} className="stat-card">
            <div className="stat-value">{s.count}</div>
            <div className="stat-label"><StatusBadge value={s.type} /></div>
          </div>
        ))}
      </div>

      {/* Property statuses */}
      <h3 className="mb-16">Объекты по статусам</h3>
      <div className="stats-grid mb-24">
        {stats.propertyStats.map(s => (
          <div key={s.status} className="stat-card">
            <div className="stat-value">{s.count}</div>
            <div className="stat-label"><StatusBadge value={s.status} /></div>
          </div>
        ))}
      </div>

      {/* Booking statuses */}
      <h3 className="mb-16">Бронирования по статусам</h3>
      <div className="stats-grid mb-24">
        {stats.bookingsByStatus.map(s => (
          <div key={s.status} className="stat-card">
            <div className="stat-value">{s.count}</div>
            <div className="stat-label"><StatusBadge value={s.status} /></div>
          </div>
        ))}
      </div>

      {/* Agent breakdown */}
      {stats.agentStats.length > 0 && (
        <>
          <h3 className="mb-16">Статистика по агентам</h3>
          <div className="table-container mb-24">
            <table className="table">
              <thead>
                <tr>
                  <th>Агент</th>
                  <th>Всего объектов</th>
                  <th>Доступно</th>
                  <th>На модерации</th>
                  <th>Продано</th>
                  <th>Выручка</th>
                </tr>
              </thead>
              <tbody>
                {stats.agentStats.map((a, i) => (
                  <tr key={i}>
                    <td style={{ fontWeight: 600 }}>{a.agentName}</td>
                    <td>{a.totalProperties}</td>
                    <td><span style={{ color: 'var(--success)', fontWeight: 600 }}>{a.availableCount}</span></td>
                    <td>
                      {a.pendingCount > 0
                        ? <span style={{ color: '#856404', fontWeight: 600 }}>⏳ {a.pendingCount}</span>
                        : '—'}
                    </td>
                    <td>{a.soldCount}</td>
                    <td style={{ fontWeight: 700, color: 'var(--primary)' }}>
                      {a.revenue > 0 ? `${a.revenue.toLocaleString('ru-RU')} ₽` : '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {/* Monthly revenue */}
      {stats.monthlyRevenue.length > 0 && (
        <>
          <h3 className="mb-16">Помесячная выручка от продаж</h3>
          <div className="table-container mb-24">
            <table className="table">
              <thead>
                <tr><th>Период</th><th>Продаж</th><th>Выручка</th></tr>
              </thead>
              <tbody>
                {stats.monthlyRevenue.map(m => (
                  <tr key={m.period}>
                    <td style={{ fontWeight: 600 }}>{m.period}</td>
                    <td>{m.count}</td>
                    <td style={{ fontWeight: 700, color: 'var(--primary)' }}>{m.revenue.toLocaleString('ru-RU')} ₽</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {/* Properties by city */}
      {stats.propertiesByCity.length > 0 && (
        <>
          <h3 className="mb-16">Объекты по городам</h3>
          <div className="stats-grid">
            {stats.propertiesByCity.map(c => (
              <div key={c.city} className="stat-card">
                <div className="stat-value">{c.count}</div>
                <div className="stat-label">{c.city}</div>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  )
}
