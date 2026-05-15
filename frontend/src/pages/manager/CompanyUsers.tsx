import { useEffect, useState } from 'react'
import { getCompanyUsers, assignAgent, assignAccountant, removeFromCompany } from '../../api/manager'

interface Employee {
  id: number
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  role: string
  isActive: boolean
}

const ROLE_LABELS: Record<string, string> = {
  Agent: 'Агент', Accountant: 'Бухгалтер', Client: 'Клиент',
}

export default function CompanyUsers() {
  const [users, setUsers] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [assignUserId, setAssignUserId] = useState('')
  const [assignRole, setAssignRole] = useState('agent')
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = () => {
    setLoading(true)
    getCompanyUsers()
      .then(r => setUsers(r.data))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const handleAssign = async () => {
    const id = parseInt(assignUserId)
    if (!id) { setMsg({ type: 'error', text: 'Введите корректный ID пользователя' }); return }
    try {
      if (assignRole === 'agent') await assignAgent(id)
      else await assignAccountant(id)
      setMsg({ type: 'success', text: 'Роль назначена' })
      setAssignUserId('')
      load()
    } catch (e: unknown) {
      const m = (e as { response?: { data?: { message?: string } } }).response?.data?.message ?? 'Ошибка'
      setMsg({ type: 'error', text: m })
    }
  }

  const handleRemove = async (id: number) => {
    if (!confirm('Снять роль и отвязать от компании?')) return
    try {
      await removeFromCompany(id)
      setMsg({ type: 'success', text: 'Пользователь переведён в роль Client' })
      load()
    } catch {
      setMsg({ type: 'error', text: 'Ошибка' })
    }
  }

  return (
    <div className="page-container">
      <h1 className="section-title">Сотрудники компании</h1>

      {msg && <div className={`alert alert-${msg.type}`}>{msg.text}</div>}

      {/* Assign by ID */}
      <div className="card mb-24">
        <div className="card-title mb-16">Назначить роль по ID пользователя</div>
        <div className="flex gap-12 flex-wrap">
          <div className="filter-group">
            <label>ID пользователя</label>
            <input
              type="number"
              className="form-control"
              placeholder="Введите ID..."
              value={assignUserId}
              onChange={e => setAssignUserId(e.target.value)}
            />
          </div>
          <div className="filter-group">
            <label>Роль</label>
            <select className="form-control" value={assignRole} onChange={e => setAssignRole(e.target.value)}>
              <option value="agent">Агент</option>
              <option value="accountant">Бухгалтер</option>
            </select>
          </div>
          <button className="btn btn-primary btn-sm" style={{ alignSelf: 'flex-end' }} onClick={handleAssign}>
            Назначить
          </button>
        </div>
        <p className="text-muted mt-8" style={{ fontSize: '0.8rem' }}>
          Пользователь должен быть зарегистрирован в системе. ID можно найти в таблице ниже.
        </p>
      </div>

      {/* Employee list */}
      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : users.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">🧑‍💼</div>
          <p className="empty-state-text">В компании нет сотрудников. Назначьте агентов или бухгалтеров выше.</p>
        </div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr><th>ID</th><th>Сотрудник</th><th>Email</th><th>Телефон</th><th>Роль</th><th>Действия</th></tr>
            </thead>
            <tbody>
              {users.map(u => (
                <tr key={u.id}>
                  <td className="text-muted">{u.id}</td>
                  <td style={{ fontWeight: 600 }}>{u.firstName} {u.lastName}</td>
                  <td>{u.email}</td>
                  <td>{u.phoneNumber ?? '—'}</td>
                  <td>{ROLE_LABELS[u.role] ?? u.role}</td>
                  <td>
                    <button className="btn btn-danger btn-sm" onClick={() => handleRemove(u.id)}>
                      Снять роль
                    </button>
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
