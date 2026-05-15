import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getCompanyStats, getCompanyClients, getCompanyBookings } from '../../api/admin'
import { StatusBadge } from '../../components/StatusBadge'
import api from '../../api/client'

interface StatsData {
  company: { id: number; name: string }
  propertyStats: { status: string; count: number }[]
  bookingStats: { type: string; count: number }[]
}

export default function CompanyView() {
  const { id } = useParams<{ id: string }>()
  const [stats, setStats] = useState<StatsData | null>(null)
  const [clients, setClients] = useState<unknown[]>([])
  const [bookings, setBookings] = useState<unknown[]>([])
  const [properties, setProperties] = useState<unknown[]>([])
  const [propTotal, setPropTotal] = useState(0)
  const [propStatusFilter, setPropStatusFilter] = useState('')
  const [tab, setTab] = useState<'stats' | 'clients' | 'bookings' | 'properties'>('stats')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!id) return
    Promise.all([
      getCompanyStats(+id).then(r => setStats(r.data)),
      getCompanyClients(+id).then(r => setClients(r.data)),
      getCompanyBookings(+id).then(r => setBookings((r.data as { bookings: unknown[] }).bookings)),
      api.get(`/admin/companies/${id}/properties`).then(r => {
        const d = r.data as { total: number; items: unknown[] }
        setProperties(d.items)
        setPropTotal(d.total)
      }),
    ]).finally(() => setLoading(false))
  }, [id])

  if (loading) return <div className="loading">Загрузка...</div>

  return (
    <div className="page-container">
      <h1 className="section-title">{stats?.company.name ?? 'Компания'}</h1>

      <div className="tabs">
        {(['stats', 'clients', 'bookings'] as const).map(t => (
          <button key={t} className={`tab-btn${tab === t ? ' active' : ''}`} onClick={() => setTab(t)}>
            {t === 'stats' ? 'Статистика' : t === 'clients' ? 'Клиенты' : 'Бронирования'}
          </button>
        ))}
        <button key="properties" className={`tab-btn${tab === 'properties' ? ' active' : ''}`} onClick={() => setTab('properties')}>
          Объекты ({propTotal})
        </button>
      </div>

      {tab === 'stats' && stats && (
        <div>
          <h3 className="mb-16">Объекты по статусам</h3>
          <div className="stats-grid mb-24">
            {stats.propertyStats.map(s => (
              <div key={s.status} className="stat-card">
                <div className="stat-value">{s.count}</div>
                <div className="stat-label"><StatusBadge value={s.status} /></div>
              </div>
            ))}
          </div>
          <h3 className="mb-16">Бронирования по типу</h3>
          <div className="stats-grid">
            {stats.bookingStats.map(s => (
              <div key={s.type} className="stat-card">
                <div className="stat-value">{s.count}</div>
                <div className="stat-label"><StatusBadge value={s.type} /></div>
              </div>
            ))}
          </div>
        </div>
      )}

      {tab === 'clients' && (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr><th>Клиент</th><th>Email</th><th>Телефон</th><th>Объект</th><th>Тип</th><th>Дата</th></tr>
            </thead>
            <tbody>
              {(clients as { clientName: string; email: string; phoneNumber?: string; propertyTitle: string; type: string; createdAt: string }[]).map((c, i) => (
                <tr key={i}>
                  <td style={{ fontWeight: 600 }}>{c.clientName}</td>
                  <td>{c.email}</td>
                  <td>{c.phoneNumber ?? '—'}</td>
                  <td>{c.propertyTitle}</td>
                  <td><StatusBadge value={c.type} /></td>
                  <td className="text-muted">{new Date(c.createdAt).toLocaleDateString('ru-RU')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {tab === 'bookings' && (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr><th>Объект</th><th>Клиент</th><th>Тип</th><th>Статус</th><th>Дата</th></tr>
            </thead>
            <tbody>
              {(bookings as { propertyTitle: string; clientName: string; clientEmail: string; type: string; status: string; createdAt: string }[]).map((b, i) => (
                <tr key={i}>
                  <td>{b.propertyTitle}</td>
                  <td>
                    <div>{b.clientName}</div>
                    <div className="text-muted">{b.clientEmail}</div>
                  </td>
                  <td><StatusBadge value={b.type} /></td>
                  <td><StatusBadge value={b.status} /></td>
                  <td className="text-muted">{new Date(b.createdAt).toLocaleDateString('ru-RU')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {tab === 'properties' && (
        <div>
          <div className="filters-bar mb-16" style={{ flexWrap: 'wrap', gap: 8 }}>
            {['', 'Available', 'PendingApproval', 'Reserved', 'Sold', 'ApprovalRejected'].map(s => (
              <button
                key={s}
                className={`btn btn-sm${propStatusFilter === s ? ' btn-primary' : ' btn-ghost'}`}
                onClick={async () => {
                  setPropStatusFilter(s)
                  const r = await api.get(`/admin/companies/${id}/properties`, { params: s ? { status: s } : {} })
                  const d = r.data as { total: number; items: unknown[] }
                  setProperties(d.items)
                  setPropTotal(d.total)
                }}
              >
                {s === '' ? 'Все' : s === 'Available' ? 'Доступные' : s === 'PendingApproval' ? 'На модерации' : s === 'Reserved' ? 'Забронированные' : s === 'Sold' ? 'Проданные' : 'Отклонённые'}
              </button>
            ))}
          </div>
          <div className="table-container">
            <table className="table">
              <thead>
                <tr><th>Название</th><th>Тип</th><th>Статус</th><th>Цена</th><th>Адрес</th><th>Агент</th><th>Дата</th></tr>
              </thead>
              <tbody>
                {(properties as { id: number; title: string; type: string; status: string; price: number; address: string; city?: string; agentName?: string; createdAt: string; rejectionReason?: string }[]).map(p => (
                  <tr key={p.id}>
                    <td>
                      <div style={{ fontWeight: 600 }}>{p.title}</div>
                      {p.rejectionReason && <div className="text-muted" style={{ fontSize: '0.8rem' }}>❌ {p.rejectionReason}</div>}
                    </td>
                    <td><StatusBadge value={p.type} /></td>
                    <td>
                      <StatusBadge value={p.status} />
                      {p.status === 'PendingApproval' && (
                        <span style={{ marginLeft: 4, fontSize: '0.75rem', background: '#fff3cd', color: '#856404', padding: '2px 6px', borderRadius: 4 }}>⏳ Ожидает</span>
                      )}
                    </td>
                    <td style={{ fontWeight: 700 }}>{p.price.toLocaleString('ru-RU')} ₽</td>
                    <td className="text-muted">{p.address}{p.city ? `, ${p.city}` : ''}</td>
                    <td>{p.agentName ?? '—'}</td>
                    <td className="text-muted">{new Date(p.createdAt).toLocaleDateString('ru-RU')}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}
