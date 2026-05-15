import { useEffect, useState } from 'react'
import { getProfile } from '../api/auth'
import type { UserInfo } from '../api/auth'
import { useAuth } from '../context/AuthContext'

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Администратор', Agent: 'Агент', Client: 'Клиент',
  Seller: 'Продавец', OfficeManager: 'Менеджер', Accountant: 'Бухгалтер',
}

export default function Profile() {
  const { logout } = useAuth()
  const [profile, setProfile] = useState<UserInfo | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getProfile()
      .then(r => setProfile(r.data))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="loading">Загрузка...</div>
  if (!profile) return null

  return (
    <div className="page-container">
      <h1 className="section-title">Профиль</h1>
      <div className="card" style={{ maxWidth: 480 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 24 }}>
          <div style={{ width: 60, height: 60, borderRadius: '50%', background: 'var(--primary-light)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.8rem', flexShrink: 0 }}>
            👤
          </div>
          <div>
            <div style={{ fontWeight: 700, fontSize: '1.2rem' }}>{profile.firstName} {profile.lastName}</div>
            <span className="badge-role">{ROLE_LABELS[profile.role] ?? profile.role}</span>
          </div>
        </div>
        <div className="divider" />
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <div>
            <div className="text-muted" style={{ fontSize: '0.75rem' }}>ID</div>
            <div style={{ fontFamily: 'monospace', color: 'var(--gray-600)' }}>#{profile.id}</div>
          </div>
          <div>
            <div className="text-muted" style={{ fontSize: '0.75rem' }}>EMAIL</div>
            <div>{profile.email}</div>
          </div>
          {profile.phoneNumber && (
            <div>
              <div className="text-muted" style={{ fontSize: '0.75rem' }}>ТЕЛЕФОН</div>
              <div>{profile.phoneNumber}</div>
            </div>
          )}
          <div>
            <div className="text-muted" style={{ fontSize: '0.75rem' }}>ДАТА РЕГИСТРАЦИИ</div>
            <div>{new Date(profile.createdAt).toLocaleDateString('ru-RU')}</div>
          </div>
        </div>
        <div className="divider" />
        <button className="btn btn-danger btn-sm" onClick={logout}>Выйти из системы</button>
      </div>
    </div>
  )
}
