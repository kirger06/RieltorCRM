import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const roleCards: Record<string, { icon: string; title: string; desc: string; to: string; color?: string }[]> = {
  Admin: [
    { icon: '🏢', title: 'Компании', desc: 'Управление компаниями и их данными', to: '/admin/companies' },
    { icon: '👥', title: 'Пользователи', desc: 'Все пользователи системы', to: '/admin/users' },
    { icon: '⏳', title: 'Модерация', desc: 'Объекты, ожидающие одобрения', to: '/admin/moderation' },
  ],
  Agent: [
    { icon: '🏠', title: 'Мои объекты', desc: 'Управление листингами', to: '/agent/properties' },
    { icon: '➕', title: 'Добавить объект', desc: 'Разместить новый листинг', to: '/agent/properties/create', color: 'var(--secondary)' },
    { icon: '📋', title: 'Запросы клиентов', desc: 'Бронирования и покупки', to: '/agent/dashboard' },
  ],
  Client: [
    { icon: '🔍', title: 'Поиск жилья', desc: 'Найти квартиру или дом', to: '/' },
    { icon: '📁', title: 'Мои бронирования', desc: 'Купленные и забронированные объекты', to: '/client/bookings' },
  ],
  OfficeManager: [
    { icon: '🏠', title: 'Объекты компании', desc: 'Все объекты включая на модерации', to: '/manager/properties' },
    { icon: '👤', title: 'База клиентов', desc: 'Контакты и история покупок', to: '/manager/clients' },
    { icon: '📊', title: 'Статистика', desc: 'Показатели компании', to: '/manager/stats' },
    { icon: '📋', title: 'Бронирования', desc: 'Все сделки компании', to: '/manager/bookings' },
    { icon: '🧑‍💼', title: 'Сотрудники', desc: 'Управление ролями', to: '/manager/users' },
  ],
  Accountant: [
    { icon: '📊', title: 'Статистика', desc: 'Финансовые показатели', to: '/accountant/stats' },
    { icon: '📥', title: 'Экспорт', desc: 'Выгрузить данные в CSV', to: '/accountant/export' },
  ],
}

export default function Dashboard() {
  const { user } = useAuth()
  const cards = roleCards[user?.role ?? ''] ?? []

  return (
    <div className="page-container">
      <h1 className="section-title">
        Добро пожаловать, {user?.firstName}!
      </h1>
      <p className="text-muted mb-24">
        Вы вошли как {user?.email}
      </p>
      <div className="role-cards">
        {cards.map(c => (
          <Link
            key={c.to}
            to={c.to}
            className="role-card"
            style={c.color ? { borderLeftColor: c.color } : {}}
          >
            <div className="role-card-icon">{c.icon}</div>
            <div className="role-card-title">{c.title}</div>
            <div className="role-card-desc">{c.desc}</div>
          </Link>
        ))}
      </div>
    </div>
  )
}
