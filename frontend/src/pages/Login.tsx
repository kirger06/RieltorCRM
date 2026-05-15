import { useState, FormEvent } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { login as apiLogin } from '../api/auth'
import { useAuth } from '../context/AuthContext'

const TEST_ACCOUNTS = [
  { label: 'Администратор', email: 'admin@rioter.ru', password: 'Admin123!' },
  { label: 'Агент', email: 'agent@rioter.ru', password: 'Agent123!' },
  { label: 'Клиент', email: 'client@rioter.ru', password: 'Client123!' },
  { label: 'Менеджер', email: 'manager@rioter.ru', password: 'Manager123!' },
  { label: 'Бухгалтер', email: 'buh@rioter.ru', password: 'Buh123!' },
]

export default function Login() {
  const navigate = useNavigate()
  const { login } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const res = await apiLogin({ email, password })
      login(res.data.token, res.data.user)
      navigate('/')
    } catch {
      setError('Неверный email или пароль')
    } finally {
      setLoading(false)
    }
  }

  const fillAccount = (acc: { email: string; password: string }) => {
    setEmail(acc.email)
    setPassword(acc.password)
    setError('')
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-logo">Риелтор+</div>
        <div className="auth-subtitle">Войдите в систему</div>

        {error && <div className="alert alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input
              className="form-control"
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
              autoFocus
            />
          </div>
          <div className="form-group">
            <label>Пароль</label>
            <input
              className="form-control"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
            />
          </div>
          <button className="btn btn-primary w-full" type="submit" disabled={loading}>
            {loading ? 'Загрузка...' : 'Войти'}
          </button>
        </form>

        <div className="auth-footer">
          Нет аккаунта? <Link to="/register">Зарегистрируйтесь</Link>
        </div>

        <div className="divider" />

        <details>
          <summary style={{ cursor: 'pointer', fontSize: '0.8rem', color: 'var(--gray-400)', marginBottom: 8 }}>
            Тестовые аккаунты
          </summary>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6, marginTop: 8 }}>
            {TEST_ACCOUNTS.map(acc => (
              <button
                key={acc.email}
                type="button"
                className="btn btn-ghost btn-sm"
                style={{ justifyContent: 'flex-start', fontSize: '0.8rem' }}
                onClick={() => fillAccount(acc)}
              >
                {acc.label} — {acc.email}
              </button>
            ))}
          </div>
        </details>
      </div>
    </div>
  )
}
