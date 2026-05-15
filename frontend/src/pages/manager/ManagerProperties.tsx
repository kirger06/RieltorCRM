import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getManagerProperties, type Property } from '../../api/properties'

const STATUS_FILTERS = [
  { value: '', label: 'Все' },
  { value: 'Available', label: 'Доступные' },
  { value: 'PendingApproval', label: 'На модерации' },
  { value: 'Reserved', label: 'Забронированные' },
  { value: 'Sold', label: 'Проданные' },
  { value: 'ApprovalRejected', label: 'Отклонённые' },
]

const statusLabel: Record<string, string> = {
  Available: 'Доступен',
  Reserved: 'Забронирован',
  Sold: 'Продан',
  PendingApproval: 'На модерации',
  ApprovalRejected: 'Отклонён',
  Inactive: 'Неактивен',
}

const statusClass: Record<string, string> = {
  Available: 'badge-available',
  Reserved: 'badge-reserved',
  Sold: 'badge-sold',
  PendingApproval: 'badge-pending',
  ApprovalRejected: 'badge-rejected',
  Inactive: 'badge-sold',
}

export default function ManagerProperties() {
  const [properties, setProperties] = useState<Property[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [statusFilter, setStatusFilter] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  useEffect(() => {
    setLoading(true)
    getManagerProperties({ status: statusFilter || undefined, page, pageSize })
      .then(r => { setProperties(r.data.items); setTotal(r.data.total) })
      .finally(() => setLoading(false))
  }, [statusFilter, page])

  const totalPages = Math.ceil(total / pageSize)

  return (
    <div className="page-container">
      <h1 className="section-title">Объекты компании</h1>

      <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginBottom: 20 }}>
        {STATUS_FILTERS.map(f => (
          <button
            key={f.value}
            className={`btn btn-sm ${statusFilter === f.value ? 'btn-primary' : 'btn-ghost'}`}
            onClick={() => { setStatusFilter(f.value); setPage(1) }}
          >
            {f.label}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : properties.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">🏠</div>
          <p className="empty-state-text">Нет объектов</p>
        </div>
      ) : (
        <>
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Название</th>
                  <th>Город</th>
                  <th>Тип</th>
                  <th>Цена</th>
                  <th>Статус</th>
                  <th>Агент</th>
                  <th>Дата</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {properties.map(p => (
                  <tr key={p.id}>
                    <td>{p.id}</td>
                    <td style={{ maxWidth: 220, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {p.title}
                      {p.status === 'PendingApproval' && (
                        <span style={{ marginLeft: 6, fontSize: '0.75rem' }}>⏳</span>
                      )}
                    </td>
                    <td>{p.city ?? '—'}</td>
                    <td>{p.type}</td>
                    <td>{p.price.toLocaleString('ru-RU')} ₽</td>
                    <td>
                      <span className={`badge ${statusClass[p.status] ?? 'badge-sold'}`}>
                        {statusLabel[p.status] ?? p.status}
                      </span>
                    </td>
                    <td>{p.agentName ?? '—'}</td>
                    <td>{new Date(p.createdAt).toLocaleDateString('ru-RU')}</td>
                    <td>
                      <Link to={`/properties/${p.id}`} className="btn btn-ghost btn-sm">
                        Открыть
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {totalPages > 1 && (
            <div className="pagination">
              <button className="pagination-btn" disabled={page === 1} onClick={() => setPage(p => p - 1)}>←</button>
              <span style={{ fontSize: '0.88rem', color: 'var(--gray-600)' }}>{page} / {totalPages}</span>
              <button className="pagination-btn" disabled={page === totalPages} onClick={() => setPage(p => p + 1)}>→</button>
            </div>
          )}
        </>
      )}
    </div>
  )
}
