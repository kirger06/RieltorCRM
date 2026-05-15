import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getCompanies, createCompany } from '../../api/admin'

interface Company {
  id: number
  name: string
  phone?: string
  address?: string
  createdAt: string
  employeeCount: number
}

export default function CompaniesList() {
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [showCreate, setShowCreate] = useState(false)
  const [form, setForm] = useState({ name: '', phone: '', address: '', description: '' })
  const [saving, setSaving] = useState(false)

  const load = () => {
    setLoading(true)
    getCompanies()
      .then(r => setCompanies(r.data))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const set = (key: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(f => ({ ...f, [key]: e.target.value }))

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await createCompany(form)
      setShowCreate(false)
      setForm({ name: '', phone: '', address: '', description: '' })
      load()
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="page-container">
      <div className="flex-between mb-16">
        <h1 className="section-title" style={{ margin: 0 }}>Компании</h1>
        <button className="btn btn-primary" onClick={() => setShowCreate(true)}>+ Добавить</button>
      </div>

      {loading ? (
        <div className="loading">Загрузка...</div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Компания</th>
                <th>Телефон</th>
                <th>Сотрудников</th>
                <th>Создана</th>
                <th>Действия</th>
              </tr>
            </thead>
            <tbody>
              {companies.map(c => (
                <tr key={c.id}>
                  <td>
                    <div style={{ fontWeight: 600 }}>{c.name}</div>
                    {c.address && <div className="text-muted">{c.address}</div>}
                  </td>
                  <td>{c.phone ?? <span className="text-muted">—</span>}</td>
                  <td>{c.employeeCount}</td>
                  <td className="text-muted">{new Date(c.createdAt).toLocaleDateString('ru-RU')}</td>
                  <td>
                    <Link to={`/admin/companies/${c.id}`} className="btn btn-ghost btn-sm">Подробнее →</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showCreate && (
        <div className="modal-overlay" onClick={() => setShowCreate(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-title">Новая компания</div>
            <form onSubmit={handleCreate}>
              <div className="form-group">
                <label>Название *</label>
                <input className="form-control" value={form.name} onChange={set('name')} required autoFocus />
              </div>
              <div className="form-group">
                <label>Телефон</label>
                <input className="form-control" value={form.phone} onChange={set('phone')} />
              </div>
              <div className="form-group">
                <label>Адрес</label>
                <input className="form-control" value={form.address} onChange={set('address')} />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn btn-ghost" onClick={() => setShowCreate(false)}>Отмена</button>
                <button type="submit" className="btn btn-primary" disabled={saving}>Создать</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
