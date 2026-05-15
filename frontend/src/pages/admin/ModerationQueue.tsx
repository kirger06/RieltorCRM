import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getPendingProperties, approveProperty, rejectProperty } from '../../api/admin'

interface PendingProperty {
  id: number
  title: string
  type: string
  price: number
  area?: number
  rooms?: number
  address: string
  city?: string
  description?: string
  imageUrls?: string
  agentName?: string
  agentEmail?: string
  createdAt: string
}

export default function ModerationQueue() {
  const [items, setItems] = useState<PendingProperty[]>([])
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [rejectId, setRejectId] = useState<number | null>(null)
  const [rejectReason, setRejectReason] = useState('')
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = () => {
    setLoading(true)
    getPendingProperties({ pageSize: 50 })
      .then(r => { setItems(r.data.items); setTotal(r.data.total) })
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const handleApprove = async (id: number) => {
    try {
      await approveProperty(id)
      setMsg({ type: 'success', text: 'Объект одобрен' })
      load()
    } catch {
      setMsg({ type: 'error', text: 'Ошибка' })
    }
  }

  const handleReject = async () => {
    if (!rejectId || !rejectReason.trim()) return
    try {
      await rejectProperty(rejectId, rejectReason)
      setMsg({ type: 'success', text: 'Объект отклонён' })
      setRejectId(null)
      setRejectReason('')
      load()
    } catch {
      setMsg({ type: 'error', text: 'Ошибка' })
    }
  }

  return (
    <div className="page-container">
      <h1 className="section-title">Модерация объектов ({total})</h1>

      {msg && <div className={`alert alert-${msg.type}`}>{msg.text}</div>}

      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : items.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">✅</div>
          <p className="empty-state-text">Нет объектов, ожидающих модерации</p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          {items.map(p => (
            <div key={p.id} className="card">
              <div style={{ display: 'flex', gap: 16, alignItems: 'flex-start' }}>
                {p.imageUrls && (() => {
                  const urls: string[] = JSON.parse(p.imageUrls)
                  return urls[0] ? (
                    <img src={urls[0]} alt="" style={{ width: 120, height: 80, objectFit: 'cover', borderRadius: 8, flexShrink: 0 }} />
                  ) : null
                })()}
                <div style={{ flex: 1 }}>
                  <div className="flex-between mb-8">
                    <Link to={`/properties/${p.id}`} style={{ fontWeight: 700, fontSize: '1.05rem' }}>{p.title}</Link>
                    <span className="text-muted">{new Date(p.createdAt).toLocaleDateString('ru-RU')}</span>
                  </div>
                  <div className="text-muted mb-8">
                    {p.type} • {p.rooms ? `${p.rooms} комн. • ` : ''}{p.area ? `${p.area} м² • ` : ''}
                    <strong style={{ color: 'var(--primary)' }}>{p.price.toLocaleString('ru-RU')} ₽</strong>
                  </div>
                  <div className="text-muted mb-8">📍 {p.address}{p.city ? `, ${p.city}` : ''}</div>
                  {p.description && <p style={{ fontSize: '0.88rem', color: 'var(--gray-700)', marginBottom: 8 }}>{p.description.slice(0, 200)}{p.description.length > 200 ? '...' : ''}</p>}
                  <div className="text-muted">Агент: {p.agentName ?? '—'} ({p.agentEmail ?? '—'})</div>
                </div>
                <div style={{ display: 'flex', gap: 8, flexShrink: 0 }}>
                  <button className="btn btn-secondary btn-sm" onClick={() => handleApprove(p.id)}>✅ Одобрить</button>
                  <button className="btn btn-danger btn-sm" onClick={() => setRejectId(p.id)}>❌ Отклонить</button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Reject modal */}
      {rejectId && (
        <div className="modal-overlay" onClick={() => setRejectId(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-title">Причина отклонения</div>
            <div className="form-group">
              <label>Укажите причину для агента</label>
              <textarea
                className="form-control"
                rows={3}
                value={rejectReason}
                onChange={e => setRejectReason(e.target.value)}
                autoFocus
              />
            </div>
            <div className="modal-actions">
              <button className="btn btn-ghost" onClick={() => setRejectId(null)}>Отмена</button>
              <button className="btn btn-danger" disabled={!rejectReason.trim()} onClick={handleReject}>
                Отклонить
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
