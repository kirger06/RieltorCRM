import { Link, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const roleLinks: Record<string, { label: string; to: string }[]> = {
  Admin: [
    { label: 'Дашборд', to: '/dashboard' },
    { label: 'Компании', to: '/admin/companies' },
    { label: 'Пользователи', to: '/admin/users' },
    { label: 'Модерация', to: '/admin/moderation' },
  ],
  Agent: [
    { label: 'Мои объекты', to: '/agent/properties' },
    { label: 'Добавить', to: '/agent/properties/create' },
    { label: 'Запросы', to: '/agent/dashboard' },
  ],
  Client: [
    { label: 'Поиск', to: '/' },
    { label: 'Мои брони', to: '/client/bookings' },
  ],
  OfficeManager: [
    { label: 'Объекты', to: '/manager/properties' },
    { label: 'Клиенты', to: '/manager/clients' },
    { label: 'Бронирования', to: '/manager/bookings' },
    { label: 'Статистика', to: '/manager/stats' },
    { label: 'Сотрудники', to: '/manager/users' },
  ],
  Accountant: [
    { label: 'Статистика', to: '/accountant/stats' },
    { label: 'Экспорт', to: '/accountant/export' },
  ],
}

const roleLabels: Record<string, string> = {
  Admin: 'Администратор',
  Agent: 'Агент',
  Client: 'Клиент',
  Seller: 'Продавец',
  OfficeManager: 'Менеджер',
  Accountant: 'Бухгалтер',
}

export default function Navbar() {
  const { user, logout, isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const links = roleLinks[user?.role ?? ''] ?? []

  return (
    <nav className="navbar">
      <div className="navbar-inner">
        <Link to="/" className="navbar-logo">Риелтор+</Link>
        <div className="navbar-links">
          {links.map((l) => (
            <Link
              key={l.to}
              to={l.to}
              className={`navbar-link${location.pathname === l.to ? ' active' : ''}`}
            >
              {l.label}
            </Link>
          ))}
        </div>
        <div className="navbar-user">
          {isAuthenticated && user ? (
            <>
              <span className="badge-role">{roleLabels[user.role] ?? user.role}</span>
              <Link to="/profile" className="navbar-user-name">{user.firstName}</Link>
              <button onClick={handleLogout} className="btn btn-ghost btn-sm">Выйти</button>
            </>
          ) : (
            <>
              <Link to="/login" className="btn btn-outline btn-sm">Войти</Link>
              <Link to="/register" className="btn btn-primary btn-sm">Регистрация</Link>
            </>
          )}
        </div>
      </div>
    </nav>
  )
}
