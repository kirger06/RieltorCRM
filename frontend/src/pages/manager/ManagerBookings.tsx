import { useEffect, useState } from 'react'
import { getManagerBookings } from '../../api/manager'
import { StatusBadge } from '../../components/StatusBadge'

interface BookingEntry {
  id: number
  type: string
  status: string
  createdAt: string
  clientName: string
  clientEmail: string
  clientPhone?: string
  propertyTitle: string
  propertyPrice: number
  propertyAddress: string
}

export default function ManagerBookings() {
  const [bookings, setBookings] = useState<BookingEntry[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)

  const load = (p = page) => {
    setLoading(true)
    getManagerBookings({ page: p, pageSize: 25 })
      .then(r => { setBookings((r.data as { bookings: BookingEntry[]; total: number }).bookings); setTotal((r.data as { total: number }).total) })
      .finally(() => setLoading(false))
  }

  useEffect(() => load(), [page])

  const totalPages = Math.ceil(total / 25)

  return (
    <div className="page-container">
      <h1 className="section-title">Бронирования ({total})</h1>

      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : bookings.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">📋</div>
          <p className="empty-state-text">Бронирований нет</p>
        </div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Объект</th>
                <th>Клиент</th>
                <th>Тип</th>
                <th>Статус</th>
                <th>Цена</th>
                <th>Дата</th>
              </tr>
            </thead>
            <tbody>
              {bookings.map(b => (
                <tr key={b.id}>
                  <td>
                    <div>{b.propertyTitle}</div>
                    <div className="text-muted">{b.propertyAddress}</div>
                  </td>
                  <td>
                    <div style={{ fontWeight: 600 }}>{b.clientName}</div>
                    <div className="text-muted">{b.clientEmail}</div>
                    {b.clientPhone && <a href={`tel:${b.clientPhone}`} style={{ fontSize: '0.8rem' }}>{b.clientPhone}</a>}
                  </td>
                  <td><StatusBadge value={b.type} /></td>
                  <td><StatusBadge value={b.status} /></td>
                  <td style={{ fontWeight: 700 }}>{b.propertyPrice.toLocaleString('ru-RU')} ₽</td>
                  <td className="text-muted">{new Date(b.createdAt).toLocaleDateString('ru-RU')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {totalPages > 1 && (
        <div className="pagination">
          <button className="pagination-btn" disabled={page === 1} onClick={() => setPage(p => p - 1)}>←</button>
          <span className="text-muted">Стр. {page} из {totalPages}</span>
          <button className="pagination-btn" disabled={page === totalPages} onClick={() => setPage(p => p + 1)}>→</button>
        </div>
      )}
    </div>
  )
}
