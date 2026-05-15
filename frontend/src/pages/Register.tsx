import { useState, FormEvent } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { register as apiRegister } from '../api/auth'
import { useAuth } from '../context/AuthContext'

export default function Register() {
  const navigate = useNavigate()
  const { login } = useAuth()
  const [role, setRole] = useState<'client' | 'manager'>('client')
  const [form, setForm] = useState({
    email: '', password: '', firstName: '', lastName: '', phoneNumber: '', companyName: ''
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const set = (key: string) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(f => ({ ...f, [key]: e.target.value }))

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    if (role === 'manager' && !form.companyName.trim()) {
      setError('Введите название компании')
      return
    }
    setLoading(true)
    try {
      const res = await apiRegister({
        email: form.email,
        password: form.password,
        firstName: form.firstName,
        lastName: form.lastName,
        phoneNumber: form.phoneNumber || undefined,
        registerAsManager: role === 'manager',
        companyName: role === 'manager' ? form.companyName : undefined,
      })
      login(res.data.token, res.data.user)
      navigate(role === 'manager' ? '/manager/users' : '/')
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } }).response?.data?.message
      setError(msg ?? 'Ошибка при регистрации')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-logo">Риелтор+</div>
        <div className="auth-subtitle">Создайте аккаунт</div>

        {/* Role toggle */}
        <div className="role-toggle mb-16">
          <button
            type="button"
            className={`role-toggle-btn${role === 'client' ? ' active' : ''}`}
            onClick={() => setRole('client')}
          >
            Я клиент
          </button>
          <button
            type="button"
            className={`role-toggle-btn${role === 'manager' ? ' active' : ''}`}
            onClick={() => setRole('manager')}
          >
            Я менеджер
          </button>
        </div>

        {role === 'manager' && (
          <div className="alert alert-info mb-16" style={{ fontSize: '0.85rem' }}>
            Вы создадите компанию и сможете добавлять в неё агентов и бухгалтеров
          </div>
        )}

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label>Имя</label>
              <input className="form-control" value={form.firstName} onChange={set('firstName')} required />
            </div>
            <div className="form-group">
              <label>Фамилия</label>
              <input className="form-control" value={form.lastName} onChange={set('lastName')} required />
            </div>
          </div>
          <div className="form-group">
            <label>Email</label>
            <input className="form-control" type="email" value={form.email} onChange={set('email')} required />
          </div>
          <div className="form-group">
            <label>Телефон</label>
            <input className="form-control" type="tel" value={form.phoneNumber} onChange={set('phoneNumber')} placeholder="+7..." />
          </div>
          <div className="form-group">
            <label>Пароль (минимум 6 символов)</label>
            <input className="form-control" type="password" value={form.password} onChange={set('password')} required minLength={6} />
          </div>

          {role === 'manager' && (
            <div className="form-group">
              <label>Название компании *</label>
              <input
                className="form-control"
                value={form.companyName}
                onChange={set('companyName')}
                placeholder="ООО Риелтор Плюс"
                required
              />
            </div>
          )}

          <button className="btn btn-primary w-full" type="submit" disabled={loading}>
            {loading ? 'Загрузка...' : role === 'manager' ? 'Создать компанию и аккаунт' : 'Зарегистрироваться'}
          </button>
        </form>

        <div className="auth-footer">
          Уже есть аккаунт? <Link to="/login">Войти</Link>
        </div>
      </div>
    </div>
  )
}
