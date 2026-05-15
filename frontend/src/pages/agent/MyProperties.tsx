import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getAgentProperties, updateAgentPropertyStatus, deleteAgentProperty, type Property } from '../../api/properties'
import { StatusBadge } from '../../components/StatusBadge'

const STATUSES = [
  { value: '', label: 'Все' },
  { value: '0', label: 'На модерации' },
  { value: '1', label: 'Отклонено' },
  { value: '2', label: 'Доступно' },
  { value: '3', label: 'Забронировано' },
  { value: '4', label: 'Продано' },
]

const MANUAL_STATUSES = [
  { value: 2, label: 'Доступно' },
  { value: 5, label: 'Неактивно' },
  { value: 6, label: 'Под договором' },
]

export default function MyProperties() {
  const [properties, setProperties] = useState<Property[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [statusFilter, setStatusFilter] = useState('')
  const [page, setPage] = useState(1)
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = () => {
    setLoading(true)
    getAgentProperties({ status: statusFilter || undefined, page, pageSize: 20 })
      .then(r => { setProperties(r.data.items); setTotal(r.data.total) })
      .finally(() => setLoading(false))
  }

  useEffect(load, [statusFilter, page])

  const handleStatusChange = async (id: number, status: number) => {
    try {
      await updateAgentPropertyStatus(id, status)
      setMsg({ type: 'success', text: 'Статус обновлён' })
      load()
    } catch {
      setMsg({ type: 'error', text: 'Не удалось обновить статус' })
    }
  }

  const handleDelete = async (id: number) => {
    if (!confirm('Деактивировать объект?')) return
    try {
      await deleteAgentProperty(id)
      setMsg({ type: 'success', text: 'Объект деактивирован' })
      load()
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { message?: string } } }).response?.data?.message ?? 'Ошибка'
      setMsg({ type: 'error', text: msg })
    }
  }

  return (
    <div className="page-container">
      <div className="flex-between mb-16">
        <h1 className="section-title" style={{ margin: 0 }}>Мои объекты ({total})</h1>
        <Link to="/agent/properties/create" className="btn btn-primary">+ Добавить объект</Link>
      </div>

      {msg && <div className={`alert alert-${msg.type}`}>{msg.text}</div>}

      <div className="filters-bar">
        <div className="filter-group">
          <label>Статус</label>
          <select value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setPage(1) }}>
            {STATUSES.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
          </select>
        </div>
      </div>

      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : properties.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">🏠</div>
          <p className="empty-state-text">Объектов нет</p>
          <Link to="/agent/properties/create" className="btn btn-primary">Добавить первый объект</Link>
        </div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Объект</th>
                <th>Тип</th>
                <th>Цена</th>
                <th>Статус</th>
                <th>Добавлен</th>
                <th>Действия</th>
              </tr>
            </thead>
            <tbody>
              {properties.map(p => (
                <tr key={p.id}>
                  <td>
                    <div style={{ fontWeight: 600 }}>{p.title}</div>
                    <div className="text-muted">{p.address}{p.city ? `, ${p.city}` : ''}</div>
                    {p.rejectionReason && (
                      <div className="text-danger" style={{ fontSize: '0.8rem' }}>
                        Причина отклонения: {p.rejectionReason}
                      </div>
                    )}
                  </td>
                  <td>{p.type}</td>
                  <td style={{ fontWeight: 700 }}>{p.price.toLocaleString('ru-RU')} ₽</td>
                  <td><StatusBadge value={p.status} /></td>
                  <td className="text-muted">{new Date(p.createdAt).toLocaleDateString('ru-RU')}</td>
                  <td>
                    <div className="flex gap-8">
                      <Link to={`/agent/properties/${p.id}/edit`} className="btn btn-ghost btn-sm">✏️</Link>
                      {p.status !== 'PendingApproval' && p.status !== 'Sold' && (
                        <select
                          className="form-control"
                          style={{ width: 130, padding: '5px 8px', fontSize: '0.8rem', height: 32 }}
                          value=""
                          onChange={e => handleStatusChange(p.id, +e.target.value)}
                        >
                          <option value="">Сменить...</option>
                          {MANUAL_STATUSES.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
                        </select>
                      )}
                      <button className="btn btn-danger btn-sm" onClick={() => handleDelete(p.id)}>🗑️</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
