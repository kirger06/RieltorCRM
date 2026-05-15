import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getAgentDashboard } from '../../api/properties'
import { StatusBadge } from '../../components/StatusBadge'

interface DashboardData {
  stats: { status: string; count: number }[]
  activeBookings: {
    id: number
    type: string
    status: string
    createdAt: string
    propertyTitle: string
    propertyId: number
    clientName: string
    clientEmail: string
    clientPhone?: string
  }[]
}

export default function AgentDashboard() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getAgentDashboard()
      .then(r => setData(r.data))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="loading">Загрузка...</div>

  return (
    <div className="page-container">
      <h1 className="section-title">Панель агента</h1>

      {/* Stats */}
      {data?.stats && data.stats.length > 0 && (
        <div className="stats-grid mb-24">
          {data.stats.map(s => (
            <div key={s.status} className="stat-card">
              <div className="stat-value">{s.count}</div>
              <div className="stat-label"><StatusBadge value={s.status} /></div>
            </div>
          ))}
        </div>
      )}

      {/* Active bookings */}
      <div className="flex-between mb-16">
        <h2 style={{ fontSize: '1.1rem', fontWeight: 700 }}>Активные запросы клиентов</h2>
        <Link to="/agent/properties" className="btn btn-outline btn-sm">Все объекты</Link>
      </div>

      {!data?.activeBookings || data.activeBookings.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">📋</div>
          <p className="empty-state-text">Активных запросов нет</p>
        </div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Объект</th>
                <th>Тип</th>
                <th>Клиент</th>
                <th>Телефон</th>
                <th>Дата</th>
              </tr>
            </thead>
            <tbody>
              {data.activeBookings.map(b => (
                <tr key={b.id}>
                  <td>
                    <Link to={`/properties/${b.propertyId}`}>{b.propertyTitle}</Link>
                  </td>
                  <td><StatusBadge value={b.type} /></td>
                  <td>
                    <div>{b.clientName}</div>
                    <div className="text-muted">{b.clientEmail}</div>
                  </td>
                  <td>
                    {b.clientPhone ? (
                      <a href={`tel:${b.clientPhone}`}>{b.clientPhone}</a>
                    ) : (
                      <span className="text-muted">—</span>
                    )}
                  </td>
                  <td className="text-muted">{new Date(b.createdAt).toLocaleDateString('ru-RU')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
