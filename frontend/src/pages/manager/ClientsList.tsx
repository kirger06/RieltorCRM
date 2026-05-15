import { useEffect, useState } from 'react'
import { getManagerClients } from '../../api/manager'
import { StatusBadge } from '../../components/StatusBadge'

interface ClientEntry {
  clientId: number
  clientName: string
  email: string
  phoneNumber?: string
  bookingId: number
  type: string
  status: string
  createdAt: string
  propertyTitle: string
  propertyId: number
  propertyPrice: number
  propertyAddress: string
}

export default function ClientsList() {
  const [clients, setClients] = useState<ClientEntry[]>([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')

  const load = (s = '') => {
    setLoading(true)
    getManagerClients(s || undefined)
      .then(r => setClients(r.data))
      .finally(() => setLoading(false))
  }

  useEffect(() => load(), [])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    load(search)
  }

  return (
    <div className="page-container">
      <h1 className="section-title">База клиентов</h1>

      <form className="filters-bar" onSubmit={handleSearch}>
        <div className="filter-group" style={{ flex: 1 }}>
          <label>Поиск</label>
          <input
            className="form-control"
            placeholder="Имя, email или объект..."
            value={search}
            onChange={e => setSearch(e.target.value)}
          />
        </div>
        <button type="submit" className="btn btn-primary btn-sm" style={{ alignSelf: 'flex-end' }}>Найти</button>
        <button type="button" className="btn btn-ghost btn-sm" style={{ alignSelf: 'flex-end' }} onClick={() => { setSearch(''); load('') }}>Сбросить</button>
      </form>

      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : clients.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">👤</div>
          <p className="empty-state-text">Клиентов пока нет</p>
        </div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Клиент</th>
                <th>Контакт</th>
                <th>Объект</th>
                <th>Тип</th>
                <th>Статус</th>
                <th>Цена</th>
                <th>Дата</th>
              </tr>
            </thead>
            <tbody>
              {clients.map((c, i) => (
                <tr key={i}>
                  <td>
                    <div style={{ fontWeight: 600 }}>{c.clientName}</div>
                  </td>
                  <td>
                    <div className="text-muted">{c.email}</div>
                    {c.phoneNumber && (
                      <a href={`tel:${c.phoneNumber}`} style={{ fontSize: '0.85rem' }}>{c.phoneNumber}</a>
                    )}
                  </td>
                  <td>
                    <div>{c.propertyTitle}</div>
                    <div className="text-muted">{c.propertyAddress}</div>
                  </td>
                  <td><StatusBadge value={c.type} /></td>
                  <td><StatusBadge value={c.status} /></td>
                  <td style={{ fontWeight: 700 }}>{c.propertyPrice.toLocaleString('ru-RU')} ₽</td>
                  <td className="text-muted">{new Date(c.createdAt).toLocaleDateString('ru-RU')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
