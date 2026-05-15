import { useEffect, useState } from 'react'
import { getAdminUsers, toggleUserActive } from '../../api/admin'

interface AdminUser {
  id: number
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  role: string
  isActive: boolean
  createdAt: string
  companyName?: string
}

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Администратор', Agent: 'Агент', Client: 'Клиент',
  Seller: 'Продавец', OfficeManager: 'Менеджер', Accountant: 'Бухгалтер',
}

export default function UsersList() {
  const [users, setUsers] = useState<AdminUser[]>([])
  const [total, setTotal] = useState(0)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)

  const load = (s = search, p = page) => {
    setLoading(true)
    getAdminUsers({ search: s || undefined, page: p, pageSize: 25 })
      .then(r => { setUsers(r.data.users); setTotal(r.data.total) })
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [page])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setPage(1)
    load(search, 1)
  }

  const handleToggle = async (id: number) => {
    await toggleUserActive(id)
    load()
  }

  const totalPages = Math.ceil(total / 25)

  return (
    <div className="page-container">
      <h1 className="section-title">Пользователи ({total})</h1>

      <form className="filters-bar" onSubmit={handleSearch}>
        <div className="filter-group" style={{ flex: 1 }}>
          <label>Поиск</label>
          <input
            className="form-control"
            placeholder="Имя, фамилия или email..."
            value={search}
            onChange={e => setSearch(e.target.value)}
          />
        </div>
        <button type="submit" className="btn btn-primary btn-sm" style={{ alignSelf: 'flex-end' }}>Найти</button>
      </form>

      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Пользователь</th>
                <th>Роль</th>
                <th>Компания</th>
                <th>Статус</th>
                <th>Дата</th>
                <th>Действия</th>
              </tr>
            </thead>
            <tbody>
              {users.map(u => (
                <tr key={u.id}>
                  <td>
                    <div style={{ fontWeight: 600 }}>{u.firstName} {u.lastName}</div>
                    <div className="text-muted">{u.email}</div>
                  </td>
                  <td>{ROLE_LABELS[u.role] ?? u.role}</td>
                  <td>{u.companyName ?? <span className="text-muted">—</span>}</td>
                  <td>
                    <span className={`badge ${u.isActive ? 'badge-available' : 'badge-cancelled'}`}>
                      {u.isActive ? 'Активен' : 'Отключён'}
                    </span>
                  </td>
                  <td className="text-muted">{new Date(u.createdAt).toLocaleDateString('ru-RU')}</td>
                  <td>
                    <button
                      className={`btn btn-sm ${u.isActive ? 'btn-danger' : 'btn-secondary'}`}
                      onClick={() => handleToggle(u.id)}
                    >
                      {u.isActive ? 'Отключить' : 'Включить'}
                    </button>
                  </td>
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
