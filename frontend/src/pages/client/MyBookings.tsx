import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getMyBookings, cancelBooking, type Booking } from '../../api/bookings'
import { StatusBadge } from '../../components/StatusBadge'

export default function MyBookings() {
  const [bookings, setBookings] = useState<Booking[]>([])
  const [loading, setLoading] = useState(true)
  const [cancelling, setCancelling] = useState<number | null>(null)
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = () => {
    setLoading(true)
    getMyBookings()
      .then(r => setBookings(r.data))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const handleCancel = async (id: number) => {
    if (!confirm('Отменить бронирование?')) return
    setCancelling(id)
    try {
      await cancelBooking(id)
      setMsg({ type: 'success', text: 'Бронирование отменено' })
      load()
    } catch {
      setMsg({ type: 'error', text: 'Не удалось отменить' })
    } finally {
      setCancelling(null)
    }
  }

  return (
    <div className="page-container">
      <h1 className="section-title">Мои бронирования</h1>

      {msg && <div className={`alert alert-${msg.type}`}>{msg.text}</div>}

      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : bookings.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">📋</div>
          <p className="empty-state-text">У вас пока нет бронирований</p>
          <Link to="/" className="btn btn-primary">Найти объект</Link>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          {bookings.map(b => (
            <div key={b.id} className="card">
              <div className="card-header">
                <div className="flex gap-8">
                  <StatusBadge value={b.type} />
                  <StatusBadge value={b.status} />
                </div>
                <span className="text-muted">{new Date(b.createdAt).toLocaleDateString('ru-RU')}</span>
              </div>
              {b.property && (
                <div style={{ display: 'flex', gap: 16, alignItems: 'flex-start' }}>
                  {b.property.imageUrls && (() => {
                    const urls: string[] = JSON.parse(b.property.imageUrls)
                    return urls[0] ? (
                      <img src={urls[0]} alt="" style={{ width: 100, height: 70, objectFit: 'cover', borderRadius: 8, flexShrink: 0 }} />
                    ) : null
                  })()}
                  <div style={{ flex: 1 }}>
                    <Link to={`/properties/${b.property.id}`} style={{ fontWeight: 700, fontSize: '1rem' }}>
                      {b.property.title}
                    </Link>
                    <div className="text-muted">📍 {b.property.address}</div>
                    <div style={{ fontWeight: 700, color: 'var(--primary)', marginTop: 4 }}>
                      {b.property.price.toLocaleString('ru-RU')} ₽
                    </div>
                    {b.property.agentPhone && (
                      <a href={`tel:${b.property.agentPhone}`} className="text-muted mt-4" style={{ display: 'block' }}>
                        Агент: {b.property.agentName} — {b.property.agentPhone}
                      </a>
                    )}
                  </div>
                  {b.status === 'Active' && (
                    <button
                      className="btn btn-danger btn-sm"
                      disabled={cancelling === b.id}
                      onClick={() => handleCancel(b.id)}
                    >
                      Отменить
                    </button>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
